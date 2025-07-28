using Microsoft.AspNetCore.SignalR;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Extensions;
using Villagers.GameServer.Interfaces;

namespace Villagers.GameServer.Services;

public class GameSimulationService : BackgroundService
{
    private readonly ILogger<GameSimulationService> _logger;
    private readonly IHubContext<GameHub, IGameClient> _hubContext;
    private readonly World _world;

    public GameSimulationService(ILogger<GameSimulationService> logger, IHubContext<GameHub, IGameClient> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
        _world = new World("Villagers World", TimeSpan.FromSeconds(5));
        
        // Subscribe to world tick events
        _world.TickOccurredEvent += OnWorldTick;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Game Simulation Service started - World: {WorldName}", _world.Name);

        // Start the world game loop
        await _world.Run(stoppingToken);
        
        _logger.LogInformation("Game Simulation Service stopped");
    }

    private async Task OnWorldTick(World world)
    {
        _logger.LogDebug("World tick: {TickNumber}", world.TickNumber);
        
        // Broadcast world state to all connected clients
        await _hubContext.Clients.All.WorldUpdate(world.ToDto());
    }
}