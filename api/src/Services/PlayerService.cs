using Microsoft.EntityFrameworkCore;
using Villagers.Api.Data;
using Villagers.Api.Extensions;

namespace Villagers.Api.Services;

public class PlayerService : IPlayerService
{
    private readonly ApiDbContext _context;
    private readonly IWorldRegistryService _worldRegistryService;

    public PlayerService(ApiDbContext context, IWorldRegistryService worldRegistryService)
    {
        _context = context;
        _worldRegistryService = worldRegistryService;
    }

    public async Task RegisterPlayerForWorldAsync(Guid playerId, Guid worldId)
    {
        // Validate that the world exists in the registry using the service
        var world = await _worldRegistryService.GetWorldAsync(worldId);
        if (world == null)
        {
            throw new InvalidOperationException($"World with ID {worldId} not found in registry");
        }

        var player = await _context.Users.FirstOrDefaultAsync(p => p.Id == playerId);
        if (player == null)
        {
            throw new InvalidOperationException($"Player with ID {playerId} not found");
        }

        var playerDomain = player.ToDomain();
        playerDomain.RegisterForWorld(worldId);

        // Update entity with changes from domain
        player.RegisteredWorldIds = playerDomain.RegisteredWorldIds.ToList();
        player.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }
}