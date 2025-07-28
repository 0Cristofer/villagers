using Microsoft.AspNetCore.SignalR;
using Villagers.GameServer.Interfaces;

namespace Villagers.GameServer.Services;

public class GameSimulationService : BackgroundService
{
    private readonly ILogger<GameSimulationService> _logger;
    private readonly IHubContext<GameHub, IGameClient> _hubContext;
    private int _gameTick = 0;

    public GameSimulationService(ILogger<GameSimulationService> logger, IHubContext<GameHub, IGameClient> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Game Simulation Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            _gameTick++;
            
            // Simulate game tick (resource production, building completion, etc.)
            await ProcessGameTick();
            
            _logger.LogDebug("Game tick: {GameTick}", _gameTick);
            
            // Wait 5 seconds before next tick (adjust as needed)
            await Task.Delay(5000, stoppingToken);
        }
        
        _logger.LogInformation("Game Simulation Service stopped");
    }

    private async Task ProcessGameTick()
    {
        // TODO: Process resource production
        // TODO: Check building completions
        // TODO: Process troop movements
        // TODO: Update village states
        
        // Send updates to clients
        var gameState = new { tick = _gameTick, timestamp = DateTime.UtcNow };
        await _hubContext.Clients.All.GameStateUpdate(gameState);
    }
}