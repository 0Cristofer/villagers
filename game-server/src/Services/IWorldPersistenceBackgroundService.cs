using Villagers.GameServer.Domain;

namespace Villagers.GameServer.Services;

/// <summary>
/// Background service for handling world persistence operations without blocking the game simulation.
/// </summary>
public interface IWorldPersistenceBackgroundService : IHostedService
{
    /// <summary>
    /// Enqueues a world snapshot for persistence. The actual save operation will be performed asynchronously.
    /// </summary>
    /// <param name="worldSnapshot">The world snapshot to persist</param>
    void EnqueueWorldForSave(WorldSnapshot worldSnapshot);

    /// <summary>
    /// Immediately persists a world snapshot synchronously. Use this for shutdown scenarios where you need to ensure the save completes.
    /// </summary>
    /// <param name="worldSnapshot">The world snapshot to persist</param>
    /// <returns>Task that completes when the world has been saved</returns>
    Task SaveWorldImmediatelyAsync(WorldSnapshot worldSnapshot);
}