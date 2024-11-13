using System.ComponentModel.DataAnnotations;

namespace ServiceB.Data.Models
{
    public class Edge
    {
        public int Id { get; set; }
        public int SourceNodeId { get; set; }
        public int TargetNodeId { get; set; }
        public Node SourceNode { get; set; }
        public Node TargetNode { get; set; }

        [ConcurrencyCheck]
        public Guid Version { get; set; }
    }
}
