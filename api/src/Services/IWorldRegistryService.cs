using Villagers.Api.Domain;
using Villagers.Api.Models;

namespace Villagers.Api.Services;

public interface IWorldRegistryService
{
    Task<Guid> RegisterWorldAsync(RegisterWorldRequest request);
    Task<bool> UnregisterWorldAsync(Guid worldId);
    Task<IEnumerable<WorldRegistry>> GetAllWorldsAsync();
    Task<WorldRegistry?> GetWorldAsync(Guid id);
}