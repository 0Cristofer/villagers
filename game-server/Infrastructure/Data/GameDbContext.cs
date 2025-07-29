using Microsoft.EntityFrameworkCore;
using Villagers.Shared.Entities;

namespace Villagers.GameServer.Infrastructure.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<WorldState> WorldStates { get; set; }
    public DbSet<Command> Commands { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // WorldState configuration
        modelBuilder.Entity<WorldState>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LastUpdated).HasDefaultValueSql("NOW()");
        });

        // Command configuration  
        modelBuilder.Entity<Command>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.Payload).HasColumnType("jsonb");
        });
    }
}