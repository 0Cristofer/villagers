// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.ComponentModel.DataAnnotations;

namespace Villagers.Api.Entities;

public class WorldConfigEntity
{
    [Required]
    [MaxLength(100)]
    public string WorldName { get; set; } = string.Empty;
    
    [Required]
    public TimeSpan TickInterval { get; set; }
}