public class GraphRequest
{
    public required string Operation { get; set; } // e.g., "create_node", "update_node", "delete_node"
    public required Object Data { get; set; }   // Data to send to Service B
}