using Villagers.GameServer.Domain;

namespace Villagers.GameServer.Services;

public interface IWorldRegistrationService
{
    Task RegisterWorldAsync(World world);
    Task UnregisterWorldAsync(World world);
}