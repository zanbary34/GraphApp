using Microsoft.AspNetCore.Mvc;
using ServiceA.Data.Models.DTOs;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

[ApiController]
[Route("api/graph")]
public class GraphController : ControllerBase
{
    private readonly WebSocketSenderHandler _webSocketService;

    public GraphController(WebSocketSenderHandler webSocketService)
    {
        _webSocketService = webSocketService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateNode([FromBody] CreateNodeRequest request)
    {
        return await ProcessRequestAsync("create_node", request);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateNode([FromBody] UpdateNodeRequest request)
    {
        return await ProcessRequestAsync("update_node", request);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteNode([FromBody] DeleteNodeRequest request)
    {
        return await ProcessRequestAsync("delete_node", request);
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetNodeTree(string name)
    {
        return await ProcessRequestAsync("get_node_with_descendants", new { Name = name });
    }

    [HttpGet("nodes")]
    public async Task<IActionResult> GetNodes()
    {
        return await ProcessRequestAsync("get_nodes");
    }

    [HttpGet("edges")]
    public async Task<IActionResult> GetEdges()
    {
        return await ProcessRequestAsync("get_edges");
    }

    private async Task<IActionResult> ProcessRequestAsync(string operation, object? data = null)
    {
        var message = new GraphRequest
        {
            Operation = operation,
            Data = data
        };

        var response = await _webSocketService.SendMessageAsync(message);
        return HandleResponse(response ?? string.Empty);
    }

    private IActionResult HandleResponse(string response)
    {
        var responseJson = JsonNode.Parse(response);

        if (responseJson is JsonObject jsonObject)
        {
            if (jsonObject["status"]?.ToString() == "error")
            {
                var errorMessage = jsonObject["message"]?.ToString() ?? "An error occurred";
                return BadRequest(new { status = "error", message = errorMessage });
            }

            return Ok(jsonObject);
        }

        if (responseJson is JsonArray jsonArray)
        {
            return Ok(jsonArray);
        }

        return Ok(response);
    }

}
