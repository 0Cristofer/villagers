using Microsoft.EntityFrameworkCore;
using Villagers.Api.Infrastructure.Data;
using Villagers.Shared.Entities;

namespace Villagers.Api.Infrastructure.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly ApiDbContext _context;

    public PlayerRepository(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<Player?> GetByIdAsync(string id)
    {
        return await _context.Players.FindAsync(id);
    }

    public async Task<Player?> GetByUsernameAsync(string username)
    {
        return await _context.Players
            .FirstOrDefaultAsync(p => p.Username == username);
    }

    public async Task<Player?> GetByEmailAsync(string email)
    {
        return await _context.Players
            .FirstOrDefaultAsync(p => p.Email == email);
    }

    public async Task<Player> CreateAsync(Player player)
    {
        player.CreatedAt = DateTime.UtcNow;
        player.UpdatedAt = DateTime.UtcNow;
        
        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        return player;
    }

    public async Task<Player> UpdateAsync(Player player)
    {
        player.UpdatedAt = DateTime.UtcNow;
        
        _context.Players.Update(player);
        await _context.SaveChangesAsync();
        return player;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.Players.AnyAsync(p => p.Id == id);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Players.AnyAsync(p => p.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Players.AnyAsync(p => p.Email == email);
    }
}