using Villagers.GameServer.Domain;

namespace Villagers.GameServer.Infrastructure.Repositories;

public interface IWorldRepository
{
    Task<World?> GetCurrentWorldAsync();
    Task SaveWorldStateAsync(World world);
}