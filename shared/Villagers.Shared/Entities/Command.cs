using System.ComponentModel.DataAnnotations;

namespace Villagers.Shared.Entities;

public class Command
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    public string Payload { get; set; } = string.Empty; // JSON serialized command data
    
    [Required]
    [MaxLength(50)]
    public string PlayerId { get; set; } = string.Empty;
    
    public CommandStatus Status { get; set; } = CommandStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ProcessedAt { get; set; }
    
    public string? ErrorMessage { get; set; }
}

public enum CommandStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}