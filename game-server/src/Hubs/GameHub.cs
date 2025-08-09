using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Villagers.GameServer.Domain;
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
        _ = await _gameService.ProcessCommandRequest(request);
    }

    [Authorize]
    public async Task RegisterForWorld(Guid playerId, StartingDirection startingDirection)
    {
        _logger.LogInformation("Player {PlayerId} requesting registration for this world with starting direction {StartingDirection}", playerId, startingDirection);
        
        var result = await _playerRegistrationService.RegisterPlayerAsync(playerId, startingDirection);
        
        // Only succeed if the game command was successfully enqueued
        if (result.FailureReason == RegistrationFailureReason.GameCommandEnqueueFailed)
        {
            _logger.LogError("Game command enqueue failed for player {PlayerId}: {ErrorMessage}", playerId, result.ErrorMessage);
            throw new HubException($"Registration failed: {result.ErrorMessage}");
        }
        
        // Wait for the command to be processed before returning success
        try
        {
            await result.Command!.WaitForCompletionAsync();
            _logger.LogInformation("Registration completed for player {PlayerId} - game command processed successfully", playerId);
        }
        catch (TimeoutException)
        {
            _logger.LogError("Game command timed out for player {PlayerId}", playerId);
            throw new HubException("Registration timed out - please try again");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Game command processing failed for player {PlayerId}: {Message}", playerId, ex.Message);
            throw new HubException($"Registration failed during processing: {ex.Message}");
        }
    }
}