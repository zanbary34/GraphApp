namespace ServiceA.Data.Models.DTOs
{
    public class CreateNodeRequest
    {
        public required string Name { get; set; }
        public List<int>? TargetNodeIds { get; set; }
    }
}
