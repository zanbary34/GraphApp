using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceB.Data.Models;

public class GraphContext : DbContext
{
    public GraphContext(DbContextOptions<GraphContext> options) : base(options) { }

    public DbSet<Node> Nodes { get; set; }
    public DbSet<Edge> Edges { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Edge>(entity =>
        {
            entity.HasIndex(e => new { e.SourceNodeId, e.TargetNodeId }).IsUnique();

            entity.HasOne(e => e.SourceNode)
                .WithMany()
                .HasForeignKey(e => e.SourceNodeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TargetNode)
                .WithMany()
                .HasForeignKey(e => e.TargetNodeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
