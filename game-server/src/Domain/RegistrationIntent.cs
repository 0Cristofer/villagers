using Villagers.GameServer.Domain.Enums;

namespace Villagers.GameServer.Domain;

public class RegistrationIntent
{
    public Guid Id { get; }
    public Guid PlayerId { get; }
    public StartingDirection StartingDirection { get; }
    public DateTime CreatedAt { get; }
    public DateTime LastRetryAt { get; private set; }
    public RegistrationResult? LastResult { get; private set; }

    private readonly object _processingLock = new();
    private bool _isProcessing;
    private int _retryCount;
    private TaskCompletionSource<RegistrationResult>? _processingCompletion;
    
    public RegistrationIntent(Guid playerId, StartingDirection startingDirection)
        : this(Guid.NewGuid(), playerId, startingDirection, DateTime.UtcNow, DateTime.UtcNow, 0, null)
    {
    }

    public RegistrationIntent(Guid id, Guid playerId, StartingDirection startingDirection, 
        DateTime createdAt, DateTime lastRetryAt, int retryCount, RegistrationResult? lastResult)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Intent ID cannot be empty", nameof(id));
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player ID cannot be empty", nameof(playerId));

        Id = id;
        PlayerId = playerId;
        StartingDirection = startingDirection;
        CreatedAt = createdAt;
        LastRetryAt = lastRetryAt;
        _retryCount = retryCount;
        LastResult = lastResult;
    }

    public int GetRetryCount()
    {
        lock (_processingLock)
        {
            return _retryCount;
        }
    }

    public bool IsCompleted()
    {
        lock (_processingLock)
        {
            return LastResult?.IsSuccess == true;
        }
    }

    public bool TryStartProcessing()
    {
        lock (_processingLock)
        {
            if (_isProcessing || IsCompleted())
                return false;
            
            _isProcessing = true;
            _processingCompletion = new TaskCompletionSource<RegistrationResult>();
            return true;
        }
    }

    public void FinishProcessing(RegistrationResult result)
    {
        lock (_processingLock)
        {
            if (!_isProcessing)
                throw new InvalidOperationException("Cannot finish processing that has not started");
            
            _isProcessing = false;
            LastResult = result;

            if (!result.IsSuccess)
            {
                _retryCount++;
                LastRetryAt = DateTime.UtcNow;
            }
            
            _processingCompletion!.SetResult(result);
            _processingCompletion = null;
        }
    }

    public Task<RegistrationResult> WaitFinishProcessingAsync()
    {
        lock (_processingLock)
        {
            return _processingCompletion?.Task ?? Task.FromResult(LastResult ?? throw new InvalidOperationException("No result available - this should not happen"));
        }
    }

    public bool ShouldProcessNow(TimeSpan retryDelay)
    {
        // Skip if not enough time has passed since last retry
        var timeSinceLastRetry = DateTime.UtcNow - LastRetryAt;
        return timeSinceLastRetry >= retryDelay;
    }
}