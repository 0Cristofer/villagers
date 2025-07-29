using System.ComponentModel.DataAnnotations;

namespace Villagers.Shared.Entities;

public class Player
{
    [Key]
    public string Id { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}