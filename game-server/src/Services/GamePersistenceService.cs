using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;
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

    public async Task SaveWorldAndClearCommandsAsync(World world)
    {
        // Save the world state first
        await _worldRepository.SaveWorldStateAsync(world);
        
        // Clear all commands before the current world tick
        // This maintains only commands since the last world snapshot
        await _commandRepository.DeleteCommandsBeforeTickAsync(world.GetCurrentTickNumber());
    }

    public async Task<World?> GetWorldAsync()
    {
        return await _worldRepository.GetCurrentWorldAsync();
    }

    public async Task<List<List<ICommand>>> GetPersistedCommandsAsync()
    {
        // Use repository method that performs efficient database-level grouping and ordering
        return await _commandRepository.GetCommandsGroupedByTickAsync();
    }
}