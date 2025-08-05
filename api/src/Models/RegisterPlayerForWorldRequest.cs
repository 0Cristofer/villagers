using System.ComponentModel.DataAnnotations;

namespace Villagers.Api.Models;

public class RegisterPlayerForWorldRequest
{
    [Required]
    public Guid WorldId { get; set; }
}