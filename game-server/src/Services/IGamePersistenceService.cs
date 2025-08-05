using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;

namespace Villagers.GameServer.Services;

public interface IGamePersistenceService
{
    Task SaveWorldAsync(World world);
    Task<World?> GetWorldAsync();
    Task<List<List<ICommand>>> GetPersistedCommandsAsync();
}