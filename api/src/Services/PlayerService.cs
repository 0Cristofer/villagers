using Villagers.Api.Domain;
using Villagers.Api.Repositories;

namespace Villagers.Api.Services;

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IWorldRegistryService _worldRegistryService;

    public PlayerService(
        IPlayerRepository playerRepository, 
        IWorldRegistryService worldRegistryService)
    {
        _playerRepository = playerRepository;
        _worldRegistryService = worldRegistryService;
    }

    public async Task RegisterPlayerForWorldAsync(Guid playerId, Guid worldId)
    {
        var world = await _worldRegistryService.GetWorldAsync(worldId);
        if (world == null)
        {
            throw new InvalidOperationException($"World with ID {worldId} not found in registry");
        }

        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new InvalidOperationException($"Player with ID {playerId} not found");
        }

        player.RegisterForWorld(worldId);
        await _playerRepository.UpdateAsync(player);
    }

    public async Task<Player?> GetByIdAsync(Guid playerId)
    {
        return await _playerRepository.GetByIdAsync(playerId);
    }
}