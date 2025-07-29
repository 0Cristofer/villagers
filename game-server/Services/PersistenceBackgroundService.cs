namespace Villagers.GameServer.Services;

public class PersistenceBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PersistenceBackgroundService> _logger;
    private readonly TimeSpan _worldStateSaveInterval = TimeSpan.FromSeconds(30); // Save world state every 30 seconds
    private readonly TimeSpan _commandProcessInterval = TimeSpan.FromSeconds(5);  // Process commands every 5 seconds

    public PersistenceBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<PersistenceBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Persistence background service starting");
        using var scope = _scopeFactory.CreateScope();
        var persistenceService = scope.ServiceProvider.GetRequiredService<IPersistenceService>();
        await persistenceService.InitializeAsync();
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Persistence background service started");
        
        var lastWorldStateSave = DateTime.UtcNow;
        var lastCommandProcess = DateTime.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;

                // Process commands more frequently
                if (now - lastCommandProcess >= _commandProcessInterval)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var persistenceService = scope.ServiceProvider.GetRequiredService<IPersistenceService>();
                    await persistenceService.ProcessCommandsAsync();
                    lastCommandProcess = now;
                }

                // Save world state less frequently
                if (now - lastWorldStateSave >= _worldStateSaveInterval)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var persistenceService = scope.ServiceProvider.GetRequiredService<IPersistenceService>();
                    // For now, we'll use 0 as currentTick - this should be integrated with actual game state
                    await persistenceService.SaveWorldStateAsync(0, true);
                    lastWorldStateSave = now;
                }

                // Small delay to prevent tight loop
                await Task.Delay(1000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in persistence background service");
                await Task.Delay(5000, stoppingToken); // Wait before retrying
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Persistence background service stopping");
        await base.StopAsync(cancellationToken);
    }
}