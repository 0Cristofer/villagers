using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Infrastructure.Repositories;

namespace Villagers.GameServer.Services;

public class GamePersistenceService : IGamePersistenceService
{
    private readonly IWorldRepository _worldRepository;

    public GamePersistenceService(IWorldRepository worldRepository)
    {
        _worldRepository = worldRepository;
    }

    public async Task SaveWorldAsync(World world)
    {
        await _worldRepository.SaveWorldStateAsync(world);
    }

    public async Task<World?> GetWorldAsync()
    {
        var commandQueue = new CommandQueue();
        return await _worldRepository.GetCurrentWorldAsync(commandQueue);
    }
}