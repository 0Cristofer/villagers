using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Villagers.GameServer.Configuration;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands.Requests;
using Villagers.GameServer.Extensions;
using Villagers.GameServer.Interfaces;

namespace Villagers.GameServer.Services;

public class GameSimulationService : BackgroundService, IGameSimulationService
{
    private readonly ILogger<GameSimulationService> _logger;
    private readonly IHubContext<GameHub, IGameClient> _hubContext;
    private World _world;
    private readonly IWorldRegistrationService _worldRegistrationService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly WorldConfiguration _worldConfig;

    public GameSimulationService(
        ILogger<GameSimulationService> logger, 
        IHubContext<GameHub, IGameClient> hubContext,
        IOptions<WorldConfiguration> worldConfig,
        IWorldRegistrationService worldRegistrationService,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _hubContext = hubContext;
        _worldRegistrationService = worldRegistrationService;
        _serviceScopeFactory = serviceScopeFactory;
        _worldConfig = worldConfig.Value;
        
        // World will be initialized in StartAsync
        _world = null!;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeWorldAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    private async Task InitializeWorldAsync(CancellationToken cancellationToken)
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
            await ReplayPersistedCommandsAsync(gamePersistenceService, cancellationToken);
            
            // After command replay is complete, update to current configuration
            var currentConfig = _worldConfig.ToDomain();
            _world.UpdateConfiguration(currentConfig);
            _logger.LogInformation("Updated world configuration to current settings after command replay");
        }
        else
        {
            _logger.LogInformation("No persisted world found, creating new world");
            var config = _worldConfig.ToDomain();
            var commandQueue = new CommandQueue();
            _world = new World(config, commandQueue);
        }
    }

    private async Task ReplayPersistedCommandsAsync(IGamePersistenceService gamePersistenceService, CancellationToken cancellationToken)
    {
        var commandGroups = await gamePersistenceService.GetPersistedCommandsAsync();
        
        if (commandGroups.Count == 0)
        {
            return;
        }
        
        _logger.LogInformation("Replaying {GroupCount} tick groups of persisted commands...", commandGroups.Count);
        
        foreach (var tickCommands in commandGroups)
        {
            // Enqueue all commands for this tick (these already have correct tick numbers)
            foreach (var command in tickCommands)
            {
                _world.EnqueueExistingCommand(command);
            }
            
            // Run one tick to process this group of commands (skip delay for fast replay)
            await _world.Run(1, true, cancellationToken);
        }
        
        _logger.LogInformation("Command replay complete. World is now at tick {TickNumber}", _world.GetCurrentTickNumber());
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
        finally
        {
            // Unregister when stopping
            await _worldRegistrationService.UnregisterWorldAsync(_world);
            _logger.LogInformation("Game Simulation Service stopped");
        }
    }

    public async Task ProcessCommandRequest(ICommandRequest request)
    {
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
        
        // Persist the command - if this fails, the command is already enqueued but will be lost on crash
        // This is acceptable as the command hasn't been processed yet
        using var scope = _serviceScopeFactory.CreateScope();
        var gamePersistenceService = scope.ServiceProvider.GetRequiredService<IGamePersistenceService>();
        
        await gamePersistenceService.SaveCommandAsync(command);
        _logger.LogDebug("Persisted command {CommandType} from player {PlayerId} for tick {TickNumber}", 
            command.GetType().Name, command.PlayerId, command.TickNumber);
    }

    public Guid GetWorldId()
    {
        return _world.Id;
    }

    public int GetCurrentTickNumber()
    {
        return _world.GetCurrentTickNumber();
    }

    public int GetNextTickNumber()
    {
        return _world.GetNextTickNumber();
    }

    private async Task OnWorldTick(World world)
    {
        _logger.LogDebug("World tick: {TickNumber}", world.GetCurrentTickNumber());
        
        // Broadcast world state to all connected clients
        await _hubContext.Clients.All.WorldUpdate(world.ToDto());
    }

}