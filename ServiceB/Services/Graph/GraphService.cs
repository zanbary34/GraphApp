using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceB.Data;
using ServiceB.Data.Models;
using System.Text.Json;
using System.Xml.Linq;

public class GraphService : IGraphService
{
    private readonly GraphContext _context;

    public GraphService(GraphContext context)
    {
        _context = context;
    }

    private static string GetErrorMessage(string error)
    {
        return JsonSerializer.Serialize(new { status = "error", message = error });
    }

    public async Task<string?> UpdateFromMessageAsync(string message)
    {
        try
        {
            var graphMessage = JsonSerializer.Deserialize<GraphMessage>(message);

            if (graphMessage == null)
            {
                return GetErrorMessage("Invalid message format");
            }

            switch (graphMessage.Operation)
            {
                case "get_node":
                    var node = await GetNodeWithDescendants(graphMessage.Data.Name);
                    return node != null ? JsonSerializer.Serialize(node) : GetErrorMessage("Node not found");

                case "create_node":
                    await CreateNodeAsync(graphMessage.Data);
                    return JsonSerializer.Serialize(new { status = "success", message = "Node created" });

                case "update_node":
                    await UpdateNodeAsync(graphMessage.Data.Name, graphMessage.Data.NewName, graphMessage.Data.NewTargetNodeIds);
                    return JsonSerializer.Serialize(new { status = "success", message = "Node updated" });

                case "delete_node":
                    await DeleteNodeAsync(graphMessage.Data.Name);
                    return JsonSerializer.Serialize(new { status = "success", message = "Node deleted" });

                default:
                    return GetErrorMessage("Unknown operation");
            }
        }
        catch (Exception ex)
        {
            return GetErrorMessage($"Error processing message: {ex.Message}");
        }
    }

    public async Task CreateNodeAsync(GraphMessageData nodeData)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (nodeData.TargetNodeIds?.Count > 2 || _context.Nodes.Any(n => n.Name == nodeData.Name))
            {
                throw new InvalidOperationException("The node is not valid.");
            }

            var newNode = new Node { Name = nodeData.Name };
            _context.Nodes.Add(newNode);
            await _context.SaveChangesAsync();

            if (nodeData.TargetNodeIds != null)
            {
                foreach (var targetNodeId in nodeData.TargetNodeIds)
                {
                    var targetNode = await _context.Nodes.FindAsync(targetNodeId);
                    if (targetNode == null)
                    {
                        throw new InvalidOperationException($"Invalid target node ID {targetNodeId}.");
                    }

                    var edge = new Edge
                    {
                        SourceNodeId = newNode.Id,
                        TargetNodeId = targetNodeId
                    };
                    _context.Edges.Add(edge);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("Concurrency conflict detected. Please try again.");
        }
    }

    public async Task UpdateNodeAsync(string currentName, string? newName = null, List<int>? newTargetNodeIds = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var node = await _context.Nodes.FirstOrDefaultAsync(n => n.Name == currentName) ?? throw new InvalidOperationException("Node not found.");

            if (_context.Nodes.Any(n => n.Name == newName)) throw new InvalidOperationException("Node name already exists.");

            node.Name = newName ?? node.Name;

            if (newTargetNodeIds != null)
            {
                if (newTargetNodeIds.Count > 2)
                {
                    throw new InvalidOperationException("A node can have a maximum of two edges.");
                }

                // Remove existing edges for this node
                var existingEdges = _context.Edges.Where(e => e.SourceNodeId == node.Id || e.TargetNodeId == node.Id);
                _context.Edges.RemoveRange(existingEdges);

                foreach (var targetNodeId in newTargetNodeIds)
                {
                    var targetNode = await _context.Nodes.FindAsync(targetNodeId);
                    if (targetNode == null) throw new InvalidOperationException("Invalid target node ID.");

                    var edge = new Edge
                    {
                        SourceNodeId = node.Id,
                        TargetNodeId = targetNodeId
                    };
                    _context.Edges.Add(edge);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("Concurrency conflict detected. Please try again.");
        }
    }

    private async Task DeleteNodeAsync(string name)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var node = await _context.Nodes.FirstOrDefaultAsync(n => n.Name == name);
            if (node == null) throw new InvalidOperationException("Node not found.");

            var edges = _context.Edges.Where(e => e.SourceNodeId == node.Id || e.TargetNodeId == node.Id);
            _context.Edges.RemoveRange(edges);
            _context.Nodes.Remove(node);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("Concurrency conflict detected. Please try again.");
        }
    }


    private async Task<DescendantsResponseDto> GetNodeWithDescendants(string name)
    {
        var node = await _context.Nodes.FirstOrDefaultAsync(n => n.Name == name);
        if (node == null) throw new InvalidOperationException("Node not found.");

        var result = new DescendantsResponseDto();
        var visitedNodes = new HashSet<int>();
        var visitedEdges = new HashSet<string>(); // Shared across recursive calls
        await TraverseGraph(node.Id, visitedNodes, visitedEdges, result);

        return result;
    }

    private async Task TraverseGraph(int nodeId, HashSet<int> visitedNodes, HashSet<string> visitedEdges, DescendantsResponseDto result)
    {
        if (visitedNodes.Contains(nodeId)) return;

        visitedNodes.Add(nodeId);
        var node = await _context.Nodes.FindAsync(nodeId);
        if (node != null)
        {
            result.Nodes.Add(new NodeDto { Id = node.Id, Name = node.Name });
        }

        var edges = await _context.Edges
            .Where(e => e.SourceNodeId == nodeId || e.TargetNodeId == nodeId)
            .ToListAsync();

        foreach (var edge in edges)
        {
            var sourceId = edge.SourceNodeId;
            var targetId = edge.TargetNodeId;

            // Create a unique key for the edge to avoid duplicates
            var edgeKey = sourceId < targetId
                ? $"{sourceId}-{targetId}"
                : $"{targetId}-{sourceId}";

            if (visitedEdges.Contains(edgeKey)) continue;
            visitedEdges.Add(edgeKey);

            result.Edges.Add(new EdgeDto { SourceNodeId = edge.SourceNodeId, TargetNodeId = edge.TargetNodeId });

            var connectedNodeId = edge.SourceNodeId == nodeId ? edge.TargetNodeId : edge.SourceNodeId;
            await TraverseGraph(connectedNodeId, visitedNodes, visitedEdges, result);
        }
    }
}
