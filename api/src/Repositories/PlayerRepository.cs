using Microsoft.EntityFrameworkCore;
using Villagers.Api.Data;
using Villagers.Api.Domain;
using Villagers.Api.Extensions;

namespace Villagers.Api.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly ApiDbContext _context;

    public PlayerRepository(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<Player?> GetByIdAsync(Guid playerId)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(p => p.Id == playerId);
        return entity?.ToDomain();
    }

    public async Task UpdateAsync(Player player)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(p => p.Id == player.Id);
        if (entity == null)
        {
            throw new InvalidOperationException($"Player with ID {player.Id} not found");
        }

        // Update entity with changes from domain
        entity.RegisteredWorldIds = player.RegisteredWorldIds.ToList();
        entity.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }
}