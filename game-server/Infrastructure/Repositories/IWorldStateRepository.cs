using Villagers.Shared.Entities;

namespace Villagers.GameServer.Infrastructure.Repositories;

public interface IWorldStateRepository
{
    Task<WorldState> GetCurrentStateAsync();
    Task<WorldState> UpdateStateAsync(long currentTick, bool isRunning = true);
}