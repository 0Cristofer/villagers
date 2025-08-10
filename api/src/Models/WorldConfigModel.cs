// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.ComponentModel.DataAnnotations;

namespace Villagers.Api.Models;

public class WorldConfigModel
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string WorldName { get; set; } = string.Empty;
    
    [Required]
    public TimeSpan TickInterval { get; set; }
}