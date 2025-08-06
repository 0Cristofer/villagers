namespace Villagers.GameServer.Domain.Commands.Requests;

public class TestCommandRequest : ICommandRequest
{
    public Guid PlayerId { get; }
    public DateTime Timestamp { get; }
    public string Message { get; }

    public TestCommandRequest(Guid playerId, string message)
    {
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player ID cannot be empty", nameof(playerId));
            
        PlayerId = playerId;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Timestamp = DateTime.UtcNow;
    }
}