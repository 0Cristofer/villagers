using Villagers.GameServer.Domain;

namespace Villagers.GameServer.Services;

public class RegistrationIntentBackgroundService : BackgroundService, IRegistrationIntentBackgroundService
{
    private readonly ILogger<RegistrationIntentBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private static readonly TimeSpan ProcessingInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(1);
    private static readonly int MaxRetryCount = 5;

    public RegistrationIntentBackgroundService(
        ILogger<RegistrationIntentBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Registration Intent Background Service started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingIntents(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing pending registration intents");
                }

                // Wait before next processing cycle
                await Task.Delay(ProcessingInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in Registration Intent Background Service");
            throw;
        }
        finally
        {
            _logger.LogInformation("Registration Intent Background Service stopped");
        }
    }

    private async Task ProcessPendingIntents(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var intentService = scope.ServiceProvider.GetRequiredService<IPlayerRegistrationIntentService>();

        var pendingIntents = await intentService.GetAllPendingIntentsAsync();
        
        if (pendingIntents.Count == 0)
        {
            return;
        }

        _logger.LogDebug("Found {Count} pending registration intents to process", pendingIntents.Count);

        var processedCount = 0;
        var failedCount = 0;

        foreach (var intent in pendingIntents)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            // Skip intents that have failed too many times or are too recent
            if (ShouldSkipIntent(intent))
            {
                continue;
            }

            try
            {
                await intentService.ProcessRegistrationAsync(intent);
                processedCount++;
                _logger.LogDebug("Successfully processed registration intent {IntentId} for player {PlayerId}", 
                    intent.Id, intent.PlayerId);
            }
            catch (Exception ex)
            {
                failedCount++;
                _logger.LogWarning(ex, "Failed to process registration intent {IntentId} for player {PlayerId} (attempt {RetryCount})",
                    intent.Id, intent.PlayerId, intent.RetryCount);

                // If we've exceeded max retries, log as error
                if (intent.RetryCount >= MaxRetryCount)
                {
                    _logger.LogError("Registration intent {IntentId} for player {PlayerId} has exceeded maximum retry count ({MaxRetryCount}) and requires manual intervention",
                        intent.Id, intent.PlayerId, MaxRetryCount);
                }
            }
        }

        if (processedCount > 0 || failedCount > 0)
        {
            _logger.LogInformation("Processed {ProcessedCount} registration intents successfully, {FailedCount} failed", 
                processedCount, failedCount);
        }
    }

    private bool ShouldSkipIntent(RegistrationIntent intent)
    {
        // Skip if exceeded max retries
        if (intent.RetryCount >= MaxRetryCount)
        {
            return true;
        }

        // Skip if not enough time has passed since last retry
        var timeSinceLastRetry = DateTime.UtcNow - intent.LastRetryAt;
        if (timeSinceLastRetry < RetryDelay)
        {
            return true;
        }

        return false;
    }
}