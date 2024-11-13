using System.ComponentModel.DataAnnotations;

namespace ServiceB.Data.Models
{
    public class Node
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        [ConcurrencyCheck]
        public Guid Version { get; set; }
    }
}
