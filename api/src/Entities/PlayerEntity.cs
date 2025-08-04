using Microsoft.AspNetCore.Identity;

namespace Villagers.Api.Entities;

public class PlayerEntity : IdentityUser<Guid>
{
    public List<Guid> RegisteredWorldIds { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}