using Microsoft.EntityFrameworkCore.Update.Internal;

namespace ServiceB.Data.Models
{
    public class GraphMessage
    {
        public required string Operation { get; set; } 

        public GraphMessageData Data { get; set; }     
    }

    public class GraphMessageData
    {
        public required string Name { get; set; }  
        
        public string? NewName { get; set; }

        public List<int>? TargetNodeIds { get; set; }

        public List<int>? NewTargetNodeIds { get; set; } 

    }

}