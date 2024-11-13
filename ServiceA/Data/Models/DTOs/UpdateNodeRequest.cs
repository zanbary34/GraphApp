namespace ServiceA.Data.Models.DTOs
{
    public class UpdateNodeRequest
    {
        public required string Name { get; set; }

        public string? NewName { get; set; }

        public List<int>? NewTargetNodeIds { get; set; }
    }
}
