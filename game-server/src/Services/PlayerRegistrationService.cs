using System.Collections.Concurrent;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands.Requests;
using Villagers.GameServer.Domain.Enums;
using Villagers.GameServer.Infrastructure.Repositories;

namespace Villagers.GameServer.Services;

public class PlayerRegistrationService : BackgroundService, IPlayerRegistrationService
{
    private readonly ILogger<PlayerRegistrationService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IGameSimulationService _gameSimulationService;
    
    // In-memory state 
    private readonly ConcurrentDictionary<Guid, RegistrationIntent> _pendingIntents = new();
    
    // Configuration
    private static readonly TimeSpan ProcessingInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(1);

    public PlayerRegistrationService(
        ILogger<PlayerRegistrationService> logger,
        IServiceScopeFactory serviceScopeFactory,
        HttpClient httpClient,
        IConfiguration configuration,
        IGameSimulationService gameSimulationService)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _httpClient = httpClient;
        _configuration = configuration;
        _gameSimulationService = gameSimulationService;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // Load pending intents from database into memory for crash recovery
        await LoadPendingIntentsFromDatabase();
        await base.StartAsync(cancellationToken);
    }

    // Called by GameHub for new registration requests
    public async Task<RegistrationResult> RegisterPlayerAsync(Guid playerId, StartingDirection startingDirection)
    {
        _logger.LogInformation("Player {PlayerId} requesting registration with starting direction {StartingDirection}", 
            playerId, startingDirection);

        // Get or create intent (with proper persistence)
        var intent = await GetOrCreateIntentAsync(playerId, startingDirection);
        
        // Try immediate processing
        return await TryProcessIntent(intent);
    }

    // Background service implementation
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Player Registration Service started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessPendingIntents();
                await Task.Delay(ProcessingInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in Player Registration Service");
            throw;
        }
        finally
        {
            _logger.LogInformation("Player Registration Service stopped");
        }
    }

    private async Task LoadPendingIntentsFromDatabase()
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRegistrationIntentRepository>();
            
            var pendingIntents = await repository.GetAllPendingIntentsAsync();
            
            foreach (var intent in pendingIntents)
            {
                _pendingIntents[intent.PlayerId] = intent;
            }
            
            _logger.LogInformation("Loaded {Count} pending registration intents from database", pendingIntents.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load pending intents from database");
            throw;
        }
    }

    private async Task<RegistrationIntent> GetOrCreateIntentAsync(Guid playerId, StartingDirection startingDirection)
    {
        // Check if intent already exists
        if (_pendingIntents.TryGetValue(playerId, out var existingIntent))
        {
            return existingIntent;
        }
        
        // Create new intent and persist it
        var intent = new RegistrationIntent(playerId, startingDirection);
        
        using var scope = _serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRegistrationIntentRepository>();
        await repository.CreateIntentAsync(intent);
        
        // Add to memory after successful persistence
        _pendingIntents[playerId] = intent;
        
        _logger.LogDebug("Created and persisted registration intent {IntentId} for player {PlayerId}", 
            intent.Id, playerId);
        
        return intent;
    }

    private async Task PersistIntentAsync(RegistrationIntent intent)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRegistrationIntentRepository>();
        await repository.SaveIntentAsync(intent);
    }

    private async Task DeleteIntentAsync(Guid intentId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRegistrationIntentRepository>();
        await repository.DeleteIntentAsync(intentId);
    }

    private async Task ProcessPendingIntents()
    {
        var intentsToProcess = _pendingIntents.Values
            .Where(intent => !intent.IsCompleted() && intent.ShouldProcessNow(RetryDelay))
            .ToList();

        if (intentsToProcess.Count == 0)
            return;

        _logger.LogDebug("Processing {Count} pending registration intents", intentsToProcess.Count);

        var processedCount = 0;
        var failedCount = 0;

        foreach (var intent in intentsToProcess)
        {
            var result = await TryProcessIntent(intent);
            if (result.IsSuccess) processedCount++;
            else failedCount++;
        }

        if (processedCount > 0 || failedCount > 0)
        {
            _logger.LogInformation("Processed {ProcessedCount} registration intents successfully, {FailedCount} failed", 
                processedCount, failedCount);
        }
    }

    private async Task<RegistrationResult> TryProcessIntent(RegistrationIntent intent)
    {
        if (!intent.TryStartProcessing())
        {
            // Wait for other processor to finish
            var success = await intent.WaitFinishProcessingAsync();
            return success ? RegistrationResult.Success() : (intent.LastResult ?? RegistrationResult.UnknownFailure("Processing failed"));
        }

        try
        {
            _logger.LogDebug("Processing registration intent {IntentId} for player {PlayerId}", 
                intent.Id, intent.PlayerId);

            // Step 1: Process game command (CRITICAL - must succeed for player to access world)
            var worldId = _gameSimulationService.GetWorldId();
            var request = new RegisterPlayerCommandRequest(intent.PlayerId, intent.StartingDirection);
            
            try
            {
                await _gameSimulationService.ProcessCommandRequest(request);
            }
            catch (Exception gameEx)
            {
                var result = RegistrationResult.GameCommandFailure(gameEx.Message);
                intent.FinishProcessing(result);
                await PersistIntentAsync(intent);
                
                _logger.LogError(gameEx, "Game command failed for player {PlayerId}: {Message}", intent.PlayerId, gameEx.Message);
                return result;
            }
            
            // Step 2: Register with API (can fail and retry in background)
            try
            {
                await RegisterPlayerForWorldAsync(intent.PlayerId, worldId);
            }
            catch (Exception apiEx)
            {
                var result = RegistrationResult.ApiFailure(apiEx.Message);
                intent.FinishProcessing(result);
                await PersistIntentAsync(intent);
                
                _logger.LogWarning(apiEx, "API registration failed for player {PlayerId}, will retry in background: {Message}", 
                    intent.PlayerId, apiEx.Message);
                return result;
            }
            
            // Step 3: Mark as completed and remove from memory
            _pendingIntents.TryRemove(intent.PlayerId, out _);
            
            // Step 4: Delete from database (no need to keep completed intents)
            await DeleteIntentAsync(intent.Id);
            
            var successResult = RegistrationResult.Success();
            intent.FinishProcessing(successResult);
            _logger.LogInformation("Successfully completed registration for player {PlayerId}", intent.PlayerId);
            return successResult;
        }
        catch (Exception ex)
        {
            // Unexpected error
            var result = RegistrationResult.UnknownFailure(ex.Message);
            intent.FinishProcessing(result);
            await PersistIntentAsync(intent);
            
            _logger.LogError(ex, "Unexpected error processing registration intent {IntentId} for player {PlayerId}: {Message}",
                intent.Id, intent.PlayerId, ex.Message);
            
            return result;
        }
    }


    // Legacy interface method for API registration
    public async Task RegisterPlayerForWorldAsync(Guid playerId, Guid worldId)
    {
        var apiBaseUrl = _configuration["Api:BaseUrl"];
        var apiKey = _configuration["Api:ApiKey"];
        
        if (string.IsNullOrEmpty(apiBaseUrl))
            throw new InvalidOperationException("Api:BaseUrl configuration is required for player registration");
        
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("Api:ApiKey configuration is required for player registration");

        var request = new { WorldId = worldId };

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        
        var response = await _httpClient.PostAsJsonAsync($"{apiBaseUrl}/api/internal/players/{playerId}/register-world", request);
        response.EnsureSuccessStatusCode();

        _logger.LogDebug("Successfully registered player {PlayerId} for world {WorldId} with API", playerId, worldId);
    }
}