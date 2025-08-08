using Villagers.GameServer.Domain;

namespace Villagers.GameServer.Services;

public class WorldPersistenceBackgroundService : BackgroundService, IWorldPersistenceBackgroundService
{
    private readonly ILogger<WorldPersistenceBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly object _lock = new object();
    private WorldSnapshot? _pendingWorldSnapshot;

    public WorldPersistenceBackgroundService(
        ILogger<WorldPersistenceBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public void EnqueueWorldForSave(WorldSnapshot worldSnapshot)
    {
        lock (_lock)
        {
            if (_pendingWorldSnapshot != null)
            {
                _logger.LogWarning("World persistence is taking longer than save interval. " +
                    "Previous save for world {WorldId} at tick {PendingTick} is still in progress. " +
                    "Replacing with new save request at tick {NewTick}",
                    _pendingWorldSnapshot.Id, _pendingWorldSnapshot.TickNumber, worldSnapshot.TickNumber);
            }

            _pendingWorldSnapshot = worldSnapshot;
            _logger.LogDebug("Enqueued world {WorldId} for persistence at tick {TickNumber}", 
                worldSnapshot.Id, worldSnapshot.TickNumber);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("World Persistence Background Service started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                WorldSnapshot? snapshotToSave;
                
                lock (_lock)
                {
                    snapshotToSave = _pendingWorldSnapshot;
                }

                if (snapshotToSave != null)
                {
                    await ProcessWorldSave(snapshotToSave);
                    
                    // Only clear pending snapshot after successful save
                    lock (_lock)
                    {
                        // Double-check it's still the same snapshot we just saved
                        if (_pendingWorldSnapshot == snapshotToSave)
                        {
                            _pendingWorldSnapshot = null;
                        }
                    }
                }

                // Check for work every 100ms
                await Task.Delay(100, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in World Persistence Background Service");
            throw;
        }
        finally
        {
            _logger.LogInformation("World Persistence Background Service stopped");
        }
    }

    public async Task SaveWorldImmediatelyAsync(WorldSnapshot worldSnapshot)
    {
        _logger.LogInformation("Immediate world save requested for world {WorldId} at tick {TickNumber}", 
            worldSnapshot.Id, worldSnapshot.TickNumber);

        await ProcessWorldSave(worldSnapshot);

        // Clear any pending save for this same world since we just saved it
        lock (_lock)
        {
            if (_pendingWorldSnapshot?.Id == worldSnapshot.Id)
            {
                _pendingWorldSnapshot = null;
                _logger.LogDebug("Cleared pending save for world {WorldId} after immediate save", worldSnapshot.Id);
            }
        }
    }

    private async Task ProcessWorldSave(WorldSnapshot worldSnapshot)
    {
        try
        {
            _logger.LogDebug("Starting persistence for world {WorldId} at tick {TickNumber}", 
                worldSnapshot.Id, worldSnapshot.TickNumber);

            using var scope = _serviceScopeFactory.CreateScope();
            var gamePersistenceService = scope.ServiceProvider.GetRequiredService<IGamePersistenceService>();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await gamePersistenceService.SaveWorldAndClearCommandsAsync(worldSnapshot);
            stopwatch.Stop();

            _logger.LogInformation("Successfully persisted world {WorldId} at tick {TickNumber} in {ElapsedMs}ms", 
                worldSnapshot.Id, worldSnapshot.TickNumber, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist world {WorldId} at tick {TickNumber}", 
                worldSnapshot.Id, worldSnapshot.TickNumber);
            throw; // Re-throw for immediate saves so caller knows it failed
        }
    }
}