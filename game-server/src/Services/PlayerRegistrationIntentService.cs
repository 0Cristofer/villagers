using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands.Requests;
using Villagers.GameServer.Domain.Enums;
using Villagers.GameServer.Infrastructure.Repositories;

namespace Villagers.GameServer.Services;

public class PlayerRegistrationIntentService : IPlayerRegistrationIntentService
{
    private readonly IRegistrationIntentRepository _intentRepository;
    private readonly IPlayerRegistrationService _playerRegistrationService;
    private readonly IGameSimulationService _gameSimulationService;
    private readonly ILogger<PlayerRegistrationIntentService> _logger;

    public PlayerRegistrationIntentService(
        IRegistrationIntentRepository intentRepository,
        IPlayerRegistrationService playerRegistrationService,
        IGameSimulationService gameSimulationService,
        ILogger<PlayerRegistrationIntentService> logger)
    {
        _intentRepository = intentRepository;
        _playerRegistrationService = playerRegistrationService;
        _gameSimulationService = gameSimulationService;
        _logger = logger;
    }

    public async Task<RegistrationIntent> CreateRegistrationIntentAsync(Guid playerId, StartingDirection startingDirection)
    {
        // Check if there's already a pending intent for this player
        var existingIntent = await _intentRepository.GetPendingIntentAsync(playerId);
        if (existingIntent != null)
        {
            _logger.LogInformation("Player {PlayerId} already has pending registration intent {IntentId}", 
                playerId, existingIntent.Id);
            return existingIntent;
        }

        var intent = new RegistrationIntent(playerId, startingDirection);
        await _intentRepository.CreateIntentAsync(intent);
        
        _logger.LogInformation("Created registration intent {IntentId} for player {PlayerId}", 
            intent.Id, playerId);
        
        return intent;
    }

    public async Task<RegistrationIntent?> GetPendingIntentAsync(Guid playerId)
    {
        return await _intentRepository.GetPendingIntentAsync(playerId);
    }

    public async Task ProcessRegistrationAsync(RegistrationIntent intent)
    {
        try
        {
            _logger.LogDebug("Processing registration intent {IntentId} for player {PlayerId}", 
                intent.Id, intent.PlayerId);

            // Step 1: Process game command first (fast, reliable)
            var worldId = _gameSimulationService.GetWorldId();
            var request = new RegisterPlayerCommandRequest(intent.PlayerId, intent.StartingDirection);
            await _gameSimulationService.ProcessCommandRequest(request);
            
            _logger.LogDebug("Game command processed successfully for player {PlayerId}", intent.PlayerId);

            // Step 2: Register with API (external, can fail)
            await _playerRegistrationService.RegisterPlayerForWorldAsync(intent.PlayerId, worldId);
            
            // Step 3: Mark intent as completed
            intent.MarkCompleted();
            await _intentRepository.SaveIntentAsync(intent);
            
            _logger.LogInformation("Successfully completed registration for player {PlayerId} via intent {IntentId}", 
                intent.PlayerId, intent.Id);
        }
        catch (Exception ex)
        {
            // Mark retry and save the error
            intent.MarkRetry(ex.Message);
            await _intentRepository.SaveIntentAsync(intent);
            
            _logger.LogWarning(ex, "Failed to process registration intent {IntentId} for player {PlayerId} (attempt {RetryCount})",
                intent.Id, intent.PlayerId, intent.RetryCount);
            
            throw; // Re-throw for immediate processing so caller knows it failed
        }
    }

    public async Task<List<RegistrationIntent>> GetAllPendingIntentsAsync()
    {
        return await _intentRepository.GetAllPendingIntentsAsync();
    }
}