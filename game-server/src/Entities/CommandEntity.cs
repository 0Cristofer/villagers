// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.ComponentModel.DataAnnotations;

namespace Villagers.GameServer.Entities;

public class CommandEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Payload { get; set; } = string.Empty; // JSON serialized command data
    
    [Required]
    public Guid PlayerId { get; set; }
    
    public long TickNumber { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}