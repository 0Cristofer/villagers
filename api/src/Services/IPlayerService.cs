using Villagers.Api.Domain;

namespace Villagers.Api.Services;

public interface IPlayerService
{
    Task RegisterPlayerForWorldAsync(Guid playerId, Guid worldId);
    Task<Player?> GetByIdAsync(Guid playerId);
}