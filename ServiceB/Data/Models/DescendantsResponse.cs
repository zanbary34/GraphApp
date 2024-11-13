public class NodeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class EdgeDto
{
    public int SourceNodeId { get; set; }
    public int TargetNodeId { get; set; }
}

public class DescendantsResponseDto
{
    public List<NodeDto> Nodes { get; set; } = new List<NodeDto>();
    public List<EdgeDto> Edges { get; set; } = new List<EdgeDto>();
}