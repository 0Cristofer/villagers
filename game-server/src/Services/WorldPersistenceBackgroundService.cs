using Villagers.GameServer.Domain;

namespace Villagers.GameServer.Services;

public class WorldPersistenceBackgroundService : BackgroundService, IWorldPersistenceBackgroundService
{
    private readonly ILogger<WorldPersistenceBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly object _lock = new object();
    private World? _pendingWorld;

    public WorldPersistenceBackgroundService(
        ILogger<WorldPersistenceBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public void EnqueueWorldForSave(World world)
    {
        lock (_lock)
        {
            if (_pendingWorld != null)
            {
                _logger.LogWarning("World persistence is taking longer than save interval. " +
                    "Previous save for world {WorldId} at tick {PendingTick} is still in progress. " +
                    "Replacing with new save request at tick {NewTick}",
                    _pendingWorld.Id, _pendingWorld.GetCurrentTickNumber(), world.GetCurrentTickNumber());
            }

            _pendingWorld = world;
            _logger.LogDebug("Enqueued world {WorldId} for persistence at tick {TickNumber}", 
                world.Id, world.GetCurrentTickNumber());
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("World Persistence Background Service started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                World? worldToSave;
                
                lock (_lock)
                {
                    worldToSave = _pendingWorld;
                }

                if (worldToSave != null)
                {
                    await ProcessWorldSave(worldToSave);
                    
                    // Only clear pending world after successful save
                    lock (_lock)
                    {
                        // Double-check it's still the same world we just saved
                        if (_pendingWorld == worldToSave)
                        {
                            _pendingWorld = null;
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

    public async Task SaveWorldImmediatelyAsync(World world)
    {
        _logger.LogInformation("Immediate world save requested for world {WorldId} at tick {TickNumber}", 
            world.Id, world.GetCurrentTickNumber());

        await ProcessWorldSave(world);

        // Clear any pending save for this same world since we just saved it
        lock (_lock)
        {
            if (_pendingWorld?.Id == world.Id)
            {
                _pendingWorld = null;
                _logger.LogDebug("Cleared pending save for world {WorldId} after immediate save", world.Id);
            }
        }
    }

    private async Task ProcessWorldSave(World world)
    {
        try
        {
            _logger.LogDebug("Starting persistence for world {WorldId} at tick {TickNumber}", 
                world.Id, world.GetCurrentTickNumber());

            using var scope = _serviceScopeFactory.CreateScope();
            var gamePersistenceService = scope.ServiceProvider.GetRequiredService<IGamePersistenceService>();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await gamePersistenceService.SaveWorldAndClearCommandsAsync(world);
            stopwatch.Stop();

            _logger.LogInformation("Successfully persisted world {WorldId} at tick {TickNumber} in {ElapsedMs}ms", 
                world.Id, world.GetCurrentTickNumber(), stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist world {WorldId} at tick {TickNumber}", 
                world.Id, world.GetCurrentTickNumber());
            throw; // Re-throw for immediate saves so caller knows it failed
        }
    }
}