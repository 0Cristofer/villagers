namespace Villagers.Api.Services;

public interface IPlayerService
{
    Task RegisterPlayerForWorldAsync(Guid playerId, Guid worldId);
}