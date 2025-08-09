using Villagers.GameServer.Domain.Enums;

namespace Villagers.GameServer.Services;

public interface IPlayerRegistrationService
{
    Task RegisterPlayerForWorldAsync(Guid playerId, Guid worldId);
    Task RegisterPlayerAsync(Guid playerId, StartingDirection startingDirection);
}