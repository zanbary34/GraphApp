using Microsoft.EntityFrameworkCore.Update.Internal;

namespace ServiceB.Data.Models
{
    public class GraphMessage
    {
        public required string Operation { get; set; }  // "get_node", "create_node", etc.
        public GraphMessageData Data { get; set; }      // Contains data for each operation
    }

    public class GraphMessageData
    {
        public required string Name { get; set; }  
        
        public string? NewName { get; set; }
        public List<int>? TargetNodeIds { get; set; }
        public List<int>? NewTargetNodeIds { get; set; } // For "create_node"

    }

}