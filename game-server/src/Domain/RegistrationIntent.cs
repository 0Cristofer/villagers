using Villagers.GameServer.Domain.Enums;

namespace Villagers.GameServer.Domain;

public class RegistrationIntent
{
    public Guid Id { get; }
    public Guid PlayerId { get; }
    public StartingDirection StartingDirection { get; }
    public DateTime CreatedAt { get; }
    public DateTime LastRetryAt { get; private set; }
    public string? LastError { get; private set; }

    private readonly object _processingLock = new();
    private bool _isCompleted;
    private bool _isProcessing;
    private int _retryCount;
    private TaskCompletionSource<bool>? _processingCompletion;
    public RegistrationIntent(Guid playerId, StartingDirection startingDirection)
        : this(Guid.NewGuid(), playerId, startingDirection, DateTime.UtcNow, DateTime.UtcNow, 0, false, null)
    {
    }

    public RegistrationIntent(Guid id, Guid playerId, StartingDirection startingDirection, 
        DateTime createdAt, DateTime lastRetryAt, int retryCount, bool isCompleted, string? lastError)
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
        _isCompleted = isCompleted;
        LastError = lastError;
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
            return _isCompleted;
        }
    }

    public bool TryStartProcessing()
    {
        lock (_processingLock)
        {
            if (_isProcessing || _isCompleted)
                return false;
            
            _isProcessing = true;
            _processingCompletion = new TaskCompletionSource<bool>();
            return true;
        }
    }

    public void FinishProcessing(bool success, string? error = null)
    {
        lock (_processingLock)
        {
            if (!_isProcessing)
                throw new InvalidOperationException("Cannot finish processing that has not started");
            
            _isProcessing = false;

            if (success)
            {
                _isCompleted = true;
                LastError = null;
            }
            else
            {
                _retryCount++;
                LastRetryAt = DateTime.UtcNow;
                LastError = error;
            }
            
            _processingCompletion!.SetResult(_isCompleted);
            _processingCompletion = null;
        }
    }

    public Task<bool> WaitFinishProcessingAsync()
    {
        lock (_processingLock)
        {
            return _processingCompletion?.Task ??  Task.FromResult(_isCompleted);
        }
    }

    public bool ShouldProcessNow(TimeSpan retryDelay)
    {
        // Skip if not enough time has passed since last retry
        var timeSinceLastRetry = DateTime.UtcNow - LastRetryAt;
        return timeSinceLastRetry >= retryDelay;
    }
}