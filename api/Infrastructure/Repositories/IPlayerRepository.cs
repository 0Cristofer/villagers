using Villagers.Shared.Entities;

namespace Villagers.Api.Infrastructure.Repositories;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(string id);
    Task<Player?> GetByUsernameAsync(string username);
    Task<Player?> GetByEmailAsync(string email);
    Task<Player> CreateAsync(Player player);
    Task<Player> UpdateAsync(Player player);
    Task<bool> ExistsAsync(string id);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
}