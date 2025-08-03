using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;

namespace Villagers.GameServer.Infrastructure.Repositories;

public interface IWorldRepository
{
    Task<World?> GetCurrentWorldAsync(CommandQueue commandQueue);
    Task SaveWorldStateAsync(World world);
}