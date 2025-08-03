using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Villagers.GameServer.Configuration;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Infrastructure.Data;
using Villagers.GameServer.Infrastructure.Extensions;

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

    public async Task<World?> GetCurrentWorldAsync(CommandQueue commandQueue)
    {
        var worldEntity = await _context.WorldStates.FirstOrDefaultAsync();
        
        if (worldEntity == null)
        {
            return null;
        }
        
        return worldEntity.ToDomain(commandQueue, _worldConfig);
    }

    public async Task SaveWorldStateAsync(World world)
    {
        var existingEntity = await _context.WorldStates.FirstOrDefaultAsync();
        
        if (existingEntity == null)
        {
            // Create new world entity
            var newEntity = world.ToEntity();
            _context.WorldStates.Add(newEntity);
        }
        else
        {
            // Update existing entity
            existingEntity.Id = world.Id;
            existingEntity.TickNumber = world.TickNumber;
            existingEntity.LastUpdated = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
    }
}