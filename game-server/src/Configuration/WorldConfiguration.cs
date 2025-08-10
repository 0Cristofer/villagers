// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.ComponentModel.DataAnnotations;

namespace Villagers.GameServer.Configuration;

public class WorldConfiguration
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string WorldName { get; set; } = "Villagers World";
    
    [Required]
    public TimeSpan TickInterval { get; set; } = TimeSpan.FromSeconds(1);
    
    [Required]
    public TimeSpan SaveInterval { get; set; } = TimeSpan.FromSeconds(10);
}