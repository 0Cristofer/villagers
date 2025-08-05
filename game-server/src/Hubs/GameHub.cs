using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Domain.Enums;
using Villagers.GameServer.Interfaces;
using Villagers.GameServer.Services;

namespace Villagers.GameServer;

public class GameHub : Hub<IGameClient>
{
    private readonly ILogger<GameHub> _logger;
    private readonly IGameSimulationService _gameService;
    private readonly IPlayerRegistrationService _playerRegistrationService;

    public GameHub(ILogger<GameHub> logger, IGameSimulationService gameService, IPlayerRegistrationService playerRegistrationService)
    {
        _logger = logger;
        _gameService = gameService;
        _playerRegistrationService = playerRegistrationService;
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

    [Authorize]
    public void SendTestCommand(Guid playerId, string message)
    {
        _logger.LogInformation("Received test command from player {PlayerId} via SignalR: {Message}", playerId, message);
        
        var nextTick = _gameService.GetNextTickNumber();
        var command = new TestCommand(playerId, message, nextTick);
        _gameService.EnqueueCommand(command);
    }

    [Authorize]
    public async Task RegisterForWorld(Guid playerId, StartingDirection startingDirection)
    {
        _logger.LogInformation("Player {PlayerId} requesting registration for this world with starting direction {StartingDirection}", playerId, startingDirection);
        
        // Get the world ID from the game service
        var worldId = _gameService.GetWorldId();
        
        // Update player's RegisteredWorldIds in database via API
        await _playerRegistrationService.RegisterPlayerForWorldAsync(playerId, worldId);
        
        // Enqueue command for game simulation processing
        var nextTick = _gameService.GetNextTickNumber();
        var command = new RegisterPlayerCommand(playerId, startingDirection, nextTick);
        _gameService.EnqueueCommand(command);
        
        _logger.LogInformation("Successfully processed registration for player {PlayerId} and world {WorldId} with direction {StartingDirection}", playerId, worldId, startingDirection);
    }
}