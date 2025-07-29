using System.ComponentModel.DataAnnotations;

namespace Villagers.Shared.Entities;

public class WorldState
{
    [Key]
    public int Id { get; set; } = 1; // Single row for world state
    
    public long CurrentTick { get; set; } = 0;
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public bool IsRunning { get; set; } = false;
}