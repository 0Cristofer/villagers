using Microsoft.EntityFrameworkCore;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Infrastructure.Data;
using Villagers.GameServer.Infrastructure.Extensions;

namespace Villagers.GameServer.Infrastructure.Repositories;

public class RegistrationIntentRepository : IRegistrationIntentRepository
{
    private readonly GameDbContext _context;

    public RegistrationIntentRepository(GameDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<RegistrationIntent>> GetAllPendingIntentsAsync()
    {
        var entities = await _context.RegistrationIntents
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        return entities.Select(x => x.ToDomain()).ToList();
    }

    public async Task SaveIntentAsync(RegistrationIntent intent)
    {
        var entity = await _context.RegistrationIntents
            .FirstOrDefaultAsync(x => x.Id == intent.Id);

        if (entity == null)
        {
            entity = intent.ToEntity();
            _context.RegistrationIntents.Add(entity);
        }
        else
        {
            entity.RetryCount = intent.GetRetryCount();
            entity.LastRetryAt = intent.LastRetryAt;
            entity.LastResult = intent.LastResult?.ToEntity();
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteIntentAsync(Guid intentId)
    {
        var entity = await _context.RegistrationIntents
            .FirstOrDefaultAsync(x => x.Id == intentId);

        if (entity != null)
        {
            _context.RegistrationIntents.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}