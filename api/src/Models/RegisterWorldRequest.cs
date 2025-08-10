// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.ComponentModel.DataAnnotations;

namespace Villagers.Api.Models;

public class RegisterWorldRequest
{
    [Required]
    public Guid WorldId { get; set; }
    
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string ServerEndpoint { get; set; } = string.Empty;
    
    [Required]
    public WorldConfigModel Config { get; set; } = null!;
}