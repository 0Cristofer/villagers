using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Villagers.GameServer.Domain.Commands.Requests;
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
    public async Task SendTestCommand(Guid playerId, string message)
    {
        _logger.LogInformation("Received test command from player {PlayerId} via SignalR: {Message}", playerId, message);
        
        var request = new TestCommandRequest(playerId, message);
        await _gameService.ProcessCommandRequest(request);
    }

    [Authorize]
    public async Task RegisterForWorld(Guid playerId, StartingDirection startingDirection)
    {
        _logger.LogInformation("Player {PlayerId} requesting registration for this world with starting direction {StartingDirection}", playerId, startingDirection);
        
        // Delegate to the unified player registration service
        await _playerRegistrationService.RegisterPlayerAsync(playerId, startingDirection);
        // TODO throw if command enqueue failed? cleanup solution
        _logger.LogInformation("Registration request processed for player {PlayerId}", playerId);
    }
}