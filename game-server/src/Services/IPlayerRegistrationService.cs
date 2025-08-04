namespace Villagers.GameServer.Services;

public interface IPlayerRegistrationService
{
    Task RegisterPlayerForWorldAsync(Guid playerId, Guid worldId);
}