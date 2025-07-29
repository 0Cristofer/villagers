using Microsoft.EntityFrameworkCore;
using Villagers.GameServer.Infrastructure.Data;
using Villagers.Shared.Entities;

namespace Villagers.GameServer.Infrastructure.Repositories;

public class WorldStateRepository : IWorldStateRepository
{
    private readonly GameDbContext _context;

    public WorldStateRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task<WorldState> GetCurrentStateAsync()
    {
        var worldState = await _context.WorldStates.FirstOrDefaultAsync();
        
        if (worldState == null)
        {
            // Initialize world state if it doesn't exist
            worldState = new WorldState
            {
                Id = 1,
                CurrentTick = 0,
                IsRunning = false,
                LastUpdated = DateTime.UtcNow
            };
            
            _context.WorldStates.Add(worldState);
            await _context.SaveChangesAsync();
        }
        
        return worldState;
    }

    public async Task<WorldState> UpdateStateAsync(long currentTick, bool isRunning = true)
    {
        var worldState = await GetCurrentStateAsync();
        
        worldState.CurrentTick = currentTick;
        worldState.IsRunning = isRunning;
        worldState.LastUpdated = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return worldState;
    }
}