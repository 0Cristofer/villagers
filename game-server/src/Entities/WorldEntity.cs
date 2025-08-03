using System.ComponentModel.DataAnnotations;

namespace Villagers.GameServer.Entities;

public class WorldEntity
{
    [Key]
    public int Id { get; set; } = 1; // Single row for world state
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public long TickNumber { get; set; } = 0;
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}