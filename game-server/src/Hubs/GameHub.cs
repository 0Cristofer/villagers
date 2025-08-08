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
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GameHub(ILogger<GameHub> logger, IGameSimulationService gameService, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _gameService = gameService;
        _serviceScopeFactory = serviceScopeFactory;
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
        
        using var scope = _serviceScopeFactory.CreateScope();
        var intentService = scope.ServiceProvider.GetRequiredService<IPlayerRegistrationIntentService>();
        
        // Check if there's already a pending intent for this player
        var existingIntent = await intentService.GetPendingIntentAsync(playerId);
        if (existingIntent != null)
        {
            _logger.LogInformation("Player {PlayerId} has existing registration intent {IntentId}, waiting for completion", 
                playerId, existingIntent.Id);
            
            // Try to process the existing intent
            try
            {
                await intentService.ProcessRegistrationAsync(existingIntent);
                _logger.LogInformation("Completed existing registration intent {IntentId} for player {PlayerId}", 
                    existingIntent.Id, playerId);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to complete existing intent {IntentId} for player {PlayerId}, will retry in background", 
                    existingIntent.Id, playerId);
                // Return success anyway - background service will retry
                return;
            }
        }

        // Create new registration intent and try to process it immediately
        var intent = await intentService.CreateRegistrationIntentAsync(playerId, startingDirection);
        
        try
        {
            await intentService.ProcessRegistrationAsync(intent);
            _logger.LogInformation("Successfully completed registration for player {PlayerId} via intent {IntentId}", 
                playerId, intent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to complete registration intent {IntentId} for player {PlayerId}, will retry in background", 
                intent.Id, playerId);
            // Return success anyway - the player is registered in the game simulation
            // Background service will retry the API registration
        }
        
        // Always return success to the client - if API registration failed, background service will handle it
    }
}