using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Villagers.GameServer.Configuration;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Extensions;
using Villagers.GameServer.Interfaces;

namespace Villagers.GameServer.Services;

public class GameSimulationService : BackgroundService, IGameSimulationService
{
    private readonly ILogger<GameSimulationService> _logger;
    private readonly IHubContext<GameHub, IGameClient> _hubContext;
    private readonly CommandQueue _commandQueue;
    private readonly World _world;
    private readonly IWorldRegistrationService _worldRegistrationService;

    public GameSimulationService(
        ILogger<GameSimulationService> logger, 
        IHubContext<GameHub, IGameClient> hubContext,
        IOptions<WorldConfiguration> worldConfig,
        IWorldRegistrationService worldRegistrationService)
    {
        _logger = logger;
        _hubContext = hubContext;
        _worldRegistrationService = worldRegistrationService;
        _commandQueue = new CommandQueue();
        
        var config = worldConfig.Value.ToDomain();
        _world = new World(config, _commandQueue);
        
        // Subscribe to world tick events
        _world.TickOccurredEvent += OnWorldTick;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Game Simulation Service started - World: {WorldName}", _world.Config.WorldName);

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

    public void EnqueueCommand(ICommand command)
    {
        _commandQueue.EnqueueCommand(command);
    }

    public Guid GetWorldId()
    {
        return _world.Id;
    }

    public int GetCurrentTickNumber()
    {
        return _world.TickNumber;
    }

    private async Task OnWorldTick(World world)
    {
        _logger.LogDebug("World tick: {TickNumber}", world.TickNumber);
        
        // Broadcast world state to all connected clients
        await _hubContext.Clients.All.WorldUpdate(world.ToDto());
    }

}