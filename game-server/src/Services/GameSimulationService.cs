using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Villagers.GameServer.Configuration;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Domain.Commands.Requests;
using Villagers.GameServer.Extensions;
using Villagers.GameServer.Hubs;
using Villagers.GameServer.Interfaces;

namespace Villagers.GameServer.Services;

public class GameSimulationService : BackgroundService, IGameSimulationService
{
    private readonly ILogger<GameSimulationService> _logger;
    private readonly IHubContext<GameHub, IGameClient> _hubContext;
    private readonly IWorldRegistrationService _worldRegistrationService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IWorldPersistenceBackgroundService _worldPersistenceService;
    
    private World _world;
    private readonly WorldConfiguration _worldConfig;
    private DateTime _lastSaveTimestamp = DateTime.MinValue;
    private bool _worldIsCorrupted;

    public GameSimulationService(
        ILogger<GameSimulationService> logger, 
        IHubContext<GameHub, IGameClient> hubContext,
        IOptions<WorldConfiguration> worldConfig,
        IWorldRegistrationService worldRegistrationService,
        IServiceScopeFactory serviceScopeFactory,
        IWorldPersistenceBackgroundService worldPersistenceService)
    {
        _logger = logger;
        _hubContext = hubContext;
        _worldRegistrationService = worldRegistrationService;
        _serviceScopeFactory = serviceScopeFactory;
        _worldPersistenceService = worldPersistenceService;
        _worldConfig = worldConfig.Value;
        
        // World will be initialized in StartAsync
        _world = null!;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeWorldAsync();
        await base.StartAsync(cancellationToken);
    }

    private async Task InitializeWorldAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var gamePersistenceService = scope.ServiceProvider.GetRequiredService<IGamePersistenceService>();
        
        // Try to load persisted world first
        var persistedWorld = await gamePersistenceService.GetWorldAsync();
        
        if (persistedWorld != null)
        {
            _logger.LogInformation("Found persisted world at tick {TickNumber}, loading...", persistedWorld.GetCurrentTickNumber());
            _world = persistedWorld;
            
            // Replay persisted commands using the persisted configuration
            await ReplayPersistedCommandsAsync(gamePersistenceService);
            
            // After command replay is complete, update to current configuration
            var currentConfig = _worldConfig.ToDomain();
            _world.UpdateConfiguration(currentConfig);
            _logger.LogInformation("Updated world configuration to current settings after command replay");
        }
        else
        {
            _logger.LogInformation("No persisted world found, creating new world");
            var config = _worldConfig.ToDomain();
            _world = new World(config);
        }
    }

    private async Task ReplayPersistedCommandsAsync(IGamePersistenceService gamePersistenceService)
    {
        var replayableTickGroups = await gamePersistenceService.GetReplayableCommandRequestsAsync();
        
        if (replayableTickGroups.Count == 0)
        {
            return;
        }
        
        _logger.LogInformation("Replaying {GroupCount} tick groups of persisted command requests...", replayableTickGroups.Count);
        
        foreach (var replayableTickGroup in replayableTickGroups)
        {
            // Re-enqueue each request and verify the tick numbers match
            foreach (var replayableCommand in replayableTickGroup)
            {
                var command = _world.EnqueueCommand(replayableCommand.Request);
                
                // Verify that the replayed request gets assigned the same tick number
                if (command.TickNumber != replayableCommand.ExpectedTickNumber)
                {
                    throw new InvalidOperationException(
                        $"Replay tick mismatch: Request was originally processed at tick {replayableCommand.ExpectedTickNumber} " +
                        $"but was assigned tick {command.TickNumber} during replay");
                }
            }
            
            // Run one tick to process this group of requests
            _world.Tick();
        }
        
        _logger.LogInformation("Command request replay complete. World is now at tick {TickNumber}", _world.GetCurrentTickNumber());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Game Simulation Service started - World: {WorldName}", _world.Config.WorldName);

        // Subscribe to world tick events for real-time updates
        _world.TickOccurredEvent += OnWorldTick;
        
        // Register with API
        await _worldRegistrationService.RegisterWorldAsync(_world);
        
        try
        {
            // Start the world game loop
            await _world.Run(stoppingToken);
        }
        catch (Exception ex)
        {
            _worldIsCorrupted = true;
            _logger.LogCritical(ex, "Fatal error in game simulation - world marked as corrupted, will not be saved");
            // Don't rethrow - let the service stop gracefully via StopAsync
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Game Simulation Service stopping...");
        
        // Unregister world first to prevent new players from joining during shutdown
        try
        {
            await _worldRegistrationService.UnregisterWorldAsync(_world);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unregister world during shutdown");
        }

        // Save world state immediately before shutting down to ensure no data loss
        if (_worldIsCorrupted)
        {
            _logger.LogWarning("Skipping world save during shutdown - world is in corrupted state");
        }
        else
        {
            try
            {
                _logger.LogInformation("Saving world state before shutdown...");
                var worldSnapshot = new WorldSnapshot(_world);
                await _worldPersistenceService.SaveWorldImmediatelyAsync(worldSnapshot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save world state during shutdown");
            }
        }

        // Call base StopAsync to handle the rest of the shutdown process
        await base.StopAsync(cancellationToken);
        
        _logger.LogInformation("Game Simulation Service stopped");
    }

    public async Task<ICommand> ProcessCommandRequest(ICommandRequest request)
    {
        if (_worldIsCorrupted)
        {
            _logger.LogWarning("Rejecting command request - world is in corrupted state");
            throw new InvalidOperationException("World is in corrupted state and cannot process commands");
        }

        // TODO: Performance optimization needed for high-throughput scenarios (5k+ players, 10k+ commands/sec)
        // Current approach: Direct database persistence per command
        // Potential issues:
        // - Database connection pool exhaustion under high load
        // - Individual INSERT operations create I/O bottleneck
        // - Network latency to database amplified per command
        // 
        // Future optimizations to consider:
        // - Write-Ahead Log (WAL) with background batch persistence
        // - Redis Streams for fast append + guaranteed durability
        // - In-memory buffer with periodic batch flushes (every 100ms or 50 commands)
        // - Consider async fire-and-forget with eventual consistency if game design allows
        
        // Convert request to command with atomic tick assignment and enqueue
        var command = _world.EnqueueCommand(request);
        
        // Persist the command request - if this fails, the command is already enqueued but will be lost on crash
        // This is acceptable for now, but can only be fixed with completely atomic processing (from enqueue to
        // persistence). Right now, there's a gap between enqueuing and persistence
        using var scope = _serviceScopeFactory.CreateScope();
        var gamePersistenceService = scope.ServiceProvider.GetRequiredService<IGamePersistenceService>();
        
        await gamePersistenceService.SaveCommandRequestAsync(request);
        _logger.LogDebug("Persisted command request {RequestType} from player {PlayerId} for tick {TickNumber}", 
            request.GetType().Name, request.PlayerId, request.ProcessedTickNumber);
        
        return command;
    }

    public Guid GetWorldId()
    {
        return _world.Id;
    }
    
    public bool IsPlayerRegistered(Guid playerId)
    {
        // TODO: implement proper check to see if player is registered in the world
        // This should check the world's registered players list
        return false;
    }

    private async Task OnWorldTick(World world)
    {
        _logger.LogDebug("World tick: {TickNumber}", world.GetCurrentTickNumber());
        
        // Broadcast world state to all connected clients
        await _hubContext.Clients.All.WorldUpdate(world.ToDto());
        
        var timeSinceLastSave = DateTime.UtcNow - _lastSaveTimestamp;
        
        // Check if we should save the world state (only if not corrupted)
        if (timeSinceLastSave >= _worldConfig.SaveInterval && !_worldIsCorrupted)
        {
            var worldSnapshot = new WorldSnapshot(_world);
            _worldPersistenceService.EnqueueWorldForSave(worldSnapshot);
            _lastSaveTimestamp = DateTime.UtcNow;
        }
    }
}