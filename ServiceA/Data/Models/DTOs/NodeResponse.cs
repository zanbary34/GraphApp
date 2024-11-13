using ServiceA.Data.Models.Models;

namespace ServiceA.Data.Models.DTOs
{
    public class NodeResponse
    {
        public int Id { get; set; }
        public ICollection<Edge> Edges { get; set; }
    }
}
