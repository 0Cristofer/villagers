using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands.Requests;
using Villagers.GameServer.Infrastructure.Repositories;

namespace Villagers.GameServer.Services;

public class GamePersistenceService : IGamePersistenceService
{
    private readonly IWorldRepository _worldRepository;
    private readonly ICommandRepository _commandRepository;

    public GamePersistenceService(IWorldRepository worldRepository, ICommandRepository commandRepository)
    {
        _worldRepository = worldRepository;
        _commandRepository = commandRepository;
    }

    public async Task<World?> GetWorldAsync()
    {
        return await _worldRepository.GetCurrentWorldAsync();
    }

    public async Task<List<List<ReplayableCommandRequest>>> GetReplayableCommandRequestsAsync()
    {
        // Use repository method that performs efficient database-level grouping and ordering
        return await _commandRepository.GetReplayableCommandRequestsGroupedByTickAsync();
    }

    public async Task SaveWorldAndClearCommandsAsync(WorldSnapshot worldSnapshot)
    {
        // Save the world state directly from snapshot data
        await _worldRepository.SaveWorldStateAsync(worldSnapshot);
        
        // Clear all command requests before the current world tick
        // This maintains only command requests since the last world snapshot
        await _commandRepository.DeleteCommandRequestsBeforeTickAsync(worldSnapshot.TickNumber);
    }

    public async Task SaveCommandRequestAsync(ICommandRequest commandRequest)
    {
        await _commandRepository.SaveCommandRequestAsync(commandRequest);
    }
}