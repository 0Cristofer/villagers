using Villagers.GameServer.Domain.Enums;

namespace Villagers.GameServer.Domain;

public class RegistrationIntent
{
    public Guid Id { get; private set; }
    public Guid PlayerId { get; private set; }
    public StartingDirection StartingDirection { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastRetryAt { get; private set; }
    public int RetryCount { get; private set; }
    public bool IsCompleted { get; private set; }
    public string? LastError { get; private set; }

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
        RetryCount = retryCount;
        IsCompleted = isCompleted;
        LastError = lastError;
    }

    public void MarkRetry(string? error = null)
    {
        RetryCount++;
        LastRetryAt = DateTime.UtcNow;
        LastError = error;
    }

    public void MarkCompleted()
    {
        IsCompleted = true;
        LastError = null;
    }
}