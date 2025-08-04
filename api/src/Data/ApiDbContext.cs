using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Villagers.Api.Entities;

namespace Villagers.Api.Data;

public class ApiDbContext : IdentityDbContext<PlayerEntity, IdentityRole<Guid>, Guid>
{
    public DbSet<WorldRegistryEntity> WorldRegistry { get; set; }

    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PlayerEntity configuration
        modelBuilder.Entity<PlayerEntity>(entity =>
        {
            entity.Property(e => e.RegisteredWorldIds)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(Guid.Parse)
                          .ToList()
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<List<Guid>>(
                        (c1, c2) => c1!.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
                );
            
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        });

        // WorldRegistryEntity configuration with owned Config
        modelBuilder.Entity<WorldRegistryEntity>(entity =>
        {
            entity.OwnsOne(e => e.Config, config =>
            {
                config.Property(c => c.WorldName).HasMaxLength(100).IsRequired();
                config.Property(c => c.TickInterval).IsRequired();
            });
        });
    }
}