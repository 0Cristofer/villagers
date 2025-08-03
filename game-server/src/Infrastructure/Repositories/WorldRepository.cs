using Microsoft.EntityFrameworkCore;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Infrastructure.Data;
using Villagers.GameServer.Infrastructure.Extensions;

namespace Villagers.GameServer.Infrastructure.Repositories;

public class WorldRepository : IWorldRepository
{
    private readonly GameDbContext _context;

    public WorldRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task<World?> GetCurrentWorldAsync(CommandQueue commandQueue)
    {
        var worldEntity = await _context.WorldStates.FirstOrDefaultAsync();
        
        if (worldEntity == null)
        {
            return null;
        }
        
        return worldEntity.ToDomain(commandQueue);
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
            existingEntity.Name = world.Name;
            existingEntity.TickNumber = world.TickNumber;
            existingEntity.LastUpdated = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
    }
}