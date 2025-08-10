// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.ComponentModel.DataAnnotations;

namespace Villagers.GameServer.Entities;

public class WorldEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public long TickNumber { get; set; }
    
    public WorldConfigEntity Config { get; set; } = new();
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}