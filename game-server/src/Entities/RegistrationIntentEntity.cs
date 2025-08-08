using System.ComponentModel.DataAnnotations;
using Villagers.GameServer.Domain.Enums;

namespace Villagers.GameServer.Entities;

public class RegistrationIntentEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid PlayerId { get; set; }
    
    public StartingDirection StartingDirection { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastRetryAt { get; set; } = DateTime.UtcNow;
    
    public int RetryCount { get; set; } = 0;
    
    public bool IsCompleted { get; set; } = false;
    
    public string? LastError { get; set; }
}