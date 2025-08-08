using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Villagers.GameServer.Configuration;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Infrastructure.Data;
using Villagers.GameServer.Infrastructure.Extensions;
using Villagers.GameServer.Services;

namespace Villagers.GameServer.Infrastructure.Repositories;

public class WorldRepository : IWorldRepository
{
    private readonly GameDbContext _context;
    private readonly WorldConfiguration _worldConfig;

    public WorldRepository(GameDbContext context, IOptions<WorldConfiguration> worldConfig)
    {
        _context = context;
        _worldConfig = worldConfig.Value;
    }

    public async Task<World?> GetCurrentWorldAsync()
    {
        var worldEntity = await _context.WorldStates.FirstOrDefaultAsync();
        
        if (worldEntity == null)
        {
            return null;
        }
        
        return worldEntity.ToDomain();
    }

    public async Task SaveWorldStateAsync(WorldSnapshot worldSnapshot)
    {
        var existingEntity = await _context.WorldStates.FirstOrDefaultAsync();
        
        if (existingEntity == null)
        {
            // Create new world entity from snapshot
            var newEntity = worldSnapshot.ToEntity();
            _context.WorldStates.Add(newEntity);
        }
        else
        {
            // Update existing entity from snapshot
            existingEntity.Id = worldSnapshot.Id;
            existingEntity.TickNumber = worldSnapshot.TickNumber;
            existingEntity.LastUpdated = DateTime.UtcNow;
            existingEntity.Config = worldSnapshot.Config.ToEntity();
        }
        
        await _context.SaveChangesAsync();
    }
}