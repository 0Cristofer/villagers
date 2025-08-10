using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Enums;

namespace Villagers.GameServer.Services;

public interface IPlayerRegistrationService
{
    Task<RegistrationResult> RegisterPlayerAsync(Guid playerId, StartingDirection startingDirection);
    Task<RegistrationResult?> GetExistingRegistrationAsync(Guid playerId);
}