using Microsoft.EntityFrameworkCore;
using Villagers.Api.Data;
using Villagers.Api.Domain;
using Villagers.Api.Extensions;
using Villagers.Api.Models;

namespace Villagers.Api.Services;

public class WorldRegistryService : IWorldRegistryService
{
    private readonly ApiDbContext _context;

    public WorldRegistryService(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> RegisterWorldAsync(RegisterWorldRequest request)
    {
        // Create domain object
        var worldRegistry = request.ToDomain();

        // Convert to entity and persist
        var entity = worldRegistry.ToEntity();
        _context.WorldRegistry.Add(entity);
        await _context.SaveChangesAsync();

        return worldRegistry.Id;
    }

    public async Task<bool> UnregisterWorldAsync(Guid worldId)
    {
        var entity = await _context.WorldRegistry.FirstOrDefaultAsync(w => w.WorldId == worldId);
        if (entity == null)
        {
            return false;
        }

        _context.WorldRegistry.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<WorldRegistry>> GetAllWorldsAsync()
    {
        var entities = await _context.WorldRegistry
            .OrderBy(w => w.RegisteredAt)
            .ToListAsync();
        
        return entities.Select(e => e.ToDomain());
    }

    public async Task<WorldRegistry?> GetWorldAsync(Guid worldId)
    {
        var entity = await _context.WorldRegistry.FirstOrDefaultAsync(w => w.WorldId == worldId);
        return entity?.ToDomain();
    }
}