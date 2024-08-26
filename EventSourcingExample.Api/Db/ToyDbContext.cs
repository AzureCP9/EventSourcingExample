using EventSourcingExample.Api.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingExample.Api.Db;

public class ToyDbContext : DbContext
{
    public ToyDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ToyDbEntity>()
            .HasKey(e => e.Id)
            .IsClustered();

        modelBuilder.Entity<ProjectionCheckpointDbEntity>()
            .HasKey(e => e.Id)
            .IsClustered();
    }

    public DbSet<ToyDbEntity> Toy { get; set; }
    public DbSet<ProjectionCheckpointDbEntity> ProjectionCheckpoint { get; set; }
}
