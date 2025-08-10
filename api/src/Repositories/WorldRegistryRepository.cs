using Microsoft.EntityFrameworkCore;
using Villagers.Api.Data;
using Villagers.Api.Domain;
using Villagers.Api.Extensions;

namespace Villagers.Api.Repositories;

public class WorldRegistryRepository : IWorldRegistryRepository
{
    private readonly ApiDbContext _context;

    public WorldRegistryRepository(ApiDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(WorldRegistry worldRegistry)
    {
        var entity = worldRegistry.ToEntity();
        _context.WorldRegistry.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Guid worldId)
    {
        var entity = await _context.WorldRegistry.FirstOrDefaultAsync(w => w.WorldId == worldId);
        if (entity == null)
        {
            return;
        }

        _context.WorldRegistry.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<WorldRegistry>> GetAllAsync()
    {
        var entities = await _context.WorldRegistry
            .OrderBy(w => w.RegisteredAt)
            .ToListAsync();
        
        return entities.Select(e => e.ToDomain());
    }

    public async Task<WorldRegistry?> GetByIdAsync(Guid worldId)
    {
        var entity = await _context.WorldRegistry.FirstOrDefaultAsync(w => w.WorldId == worldId);
        return entity?.ToDomain();
    }
}