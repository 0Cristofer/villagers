namespace Villagers.GameServer.Domain.Commands.Requests;

/// <summary>
/// Base class for command requests providing common validation and behavior.
/// </summary>
public abstract class BaseCommandRequest : ICommandRequest
{
    public Guid PlayerId { get; }
    public DateTime Timestamp { get; }
    public long? ProcessedTickNumber { get; private set; }

    protected BaseCommandRequest(Guid playerId)
    {
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player ID cannot be empty", nameof(playerId));
            
        PlayerId = playerId;
        Timestamp = DateTime.UtcNow;
    }

    public void SetProcessedTickNumber(long tickNumber)
    {
        if (ProcessedTickNumber.HasValue)
            throw new InvalidOperationException("ProcessedTickNumber has already been set");
            
        if (tickNumber < 0)
            throw new ArgumentException("Tick number must be non-negative", nameof(tickNumber));
            
        ProcessedTickNumber = tickNumber;
    }
}