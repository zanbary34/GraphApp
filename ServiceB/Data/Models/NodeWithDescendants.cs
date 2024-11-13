namespace ServiceB.Data.Models
{
    public class NodeWithDescendants
    {
        public int NodeId { get; set; }
        public string NodeName { get; set; }
        public List<NodeWithDescendants> Descendants { get; set; }
        public List<int> Arcs { get; set; }
    }
}
