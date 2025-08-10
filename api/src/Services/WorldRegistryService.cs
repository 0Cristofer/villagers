using Villagers.Api.Domain;
using Villagers.Api.Repositories;

namespace Villagers.Api.Services;

public class WorldRegistryService : IWorldRegistryService
{
    private readonly IWorldRegistryRepository _worldRegistryRepository;

    public WorldRegistryService(IWorldRegistryRepository worldRegistryRepository)
    {
        _worldRegistryRepository = worldRegistryRepository;
    }

    public async Task RegisterWorldAsync(WorldRegistry request)
    {
        await _worldRegistryRepository.AddAsync(request);
    }

    public async Task UnregisterWorldAsync(Guid worldId)
    {
        await _worldRegistryRepository.RemoveAsync(worldId);
    }

    public async Task<IEnumerable<WorldRegistry>> GetAllWorldsAsync()
    {
        return await _worldRegistryRepository.GetAllAsync();
    }

    public async Task<WorldRegistry?> GetWorldAsync(Guid worldId)
    {
        return await _worldRegistryRepository.GetByIdAsync(worldId);
    }
}