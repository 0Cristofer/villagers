using Microsoft.AspNetCore.SignalR;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Interfaces;
using Villagers.GameServer.Services;

namespace Villagers.GameServer;

public class GameHub : Hub<IGameClient>
{
    private readonly ILogger<GameHub> _logger;
    private readonly IGameSimulationService _gameService;

    public GameHub(ILogger<GameHub> logger, IGameSimulationService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendTestCommand(Guid playerId, string message)
    {
        _logger.LogInformation("Received test command from player {PlayerId} via SignalR: {Message}", playerId, message);
        
        var command = new TestCommand(playerId, message);
        _gameService.EnqueueCommand(command);
        
        // Acknowledge command receipt to the specific client
        await Clients.Caller.CommandReceived("TestCommand", $"Command queued for processing");
    }
}