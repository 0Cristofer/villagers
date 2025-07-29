namespace Villagers.GameServer.Services;

public interface IPersistenceService
{
    Task SaveWorldStateAsync(long currentTick, bool isRunning = true);
    Task ProcessCommandsAsync();
    Task InitializeAsync();
}