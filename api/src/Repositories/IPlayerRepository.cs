using Villagers.Api.Domain;

namespace Villagers.Api.Repositories;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid playerId);
    Task UpdateAsync(Player player);
}