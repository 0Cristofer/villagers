using Villagers.Api.Domain;

namespace Villagers.Api.Repositories;

public interface IWorldRegistryRepository
{
    Task AddAsync(WorldRegistry worldRegistry);
    Task RemoveAsync(Guid worldId);
    Task<IEnumerable<WorldRegistry>> GetAllAsync();
    Task<WorldRegistry?> GetByIdAsync(Guid worldId);
}