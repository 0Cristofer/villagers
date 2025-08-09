using Microsoft.EntityFrameworkCore;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Entities;

namespace Villagers.GameServer.Infrastructure.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<WorldEntity> WorldStates { get; set; }
    public DbSet<CommandEntity> Commands { get; set; }
    public DbSet<RegistrationIntentEntity> RegistrationIntents { get; set; }

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
            // Composite index for optimal GROUP BY TickNumber ORDER BY CreatedAt performance
            entity.HasIndex(e => new { e.TickNumber, e.CreatedAt });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.Payload).HasColumnType("jsonb");
        });

        // RegistrationIntentEntity configuration
        modelBuilder.Entity<RegistrationIntentEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PlayerId);
            entity.HasIndex(e => new { e.IsCompleted, e.CreatedAt });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.LastRetryAt).HasDefaultValueSql("NOW()");
            
            // Configure RegistrationResult as owned type
            entity.OwnsOne(e => e.LastResult, result =>
            {
                result.Property(r => r.IsSuccess).HasColumnName("LastResult_IsSuccess");
                result.Property(r => r.FailureReason).HasColumnName("LastResult_FailureReason");
                result.Property(r => r.ErrorMessage).HasColumnName("LastResult_ErrorMessage");
            });
        });
    }
}