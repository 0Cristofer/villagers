using Villagers.Api.Domain;

namespace Villagers.Api.Services;

public interface IWorldRegistryService
{
    Task RegisterWorldAsync(WorldRegistry request);
    Task UnregisterWorldAsync(Guid worldId);
    Task<IEnumerable<WorldRegistry>> GetAllWorldsAsync();
    Task<WorldRegistry?> GetWorldAsync(Guid id);
}