using Villagers.GameServer.Domain;

namespace Villagers.GameServer.Services;

public interface IGamePersistenceService
{
    Task SaveWorldAsync(World world);
    Task<World?> GetWorldAsync();
}