using Villagers.GameServer.Domain;
using Villagers.GameServer.Services;

namespace Villagers.GameServer.Infrastructure.Repositories;

public interface IWorldRepository
{
    Task<World?> GetCurrentWorldAsync();
    Task SaveWorldStateAsync(WorldSnapshot worldSnapshot);
}