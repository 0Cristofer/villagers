using System.ComponentModel.DataAnnotations;

namespace Villagers.Api.Entities;

public class WorldRegistryEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid WorldId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ServerEndpoint { get; set; } = string.Empty;
    
    [Required]
    public WorldConfigEntity Config { get; set; } = null!;
    
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}