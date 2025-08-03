using Microsoft.EntityFrameworkCore;
using Villagers.GameServer.Entities;

namespace Villagers.GameServer.Infrastructure.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<WorldEntity> WorldStates { get; set; }
    public DbSet<CommandEntity> Commands { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // WorldEntity configuration
        modelBuilder.Entity<WorldEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LastUpdated).HasDefaultValueSql("NOW()");
        });

        // CommandEntity configuration  
        modelBuilder.Entity<CommandEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.Payload).HasColumnType("jsonb");
        });
    }
}