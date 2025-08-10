// ReSharper disable PropertyCanBeMadeInitOnly.Global

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Villagers.GameServer.Domain;

namespace Villagers.GameServer.Entities;

[Owned]
public class RegistrationResultEntity
{
    public bool IsSuccess { get; set; }
    
    public RegistrationFailureReason FailureReason { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}