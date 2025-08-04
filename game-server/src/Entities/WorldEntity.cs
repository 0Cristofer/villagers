using System.ComponentModel.DataAnnotations;

namespace Villagers.GameServer.Entities;

public class WorldEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public long TickNumber { get; set; } = 0;
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}