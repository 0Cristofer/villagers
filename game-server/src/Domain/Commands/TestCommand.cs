namespace Villagers.GameServer.Domain.Commands;

public class TestCommand : ICommand
{
    public Guid PlayerId { get; }
    public DateTime Timestamp { get; }
    public string Message { get; }

    public TestCommand(Guid playerId, string message)
    {
        PlayerId = playerId;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Timestamp = DateTime.UtcNow;
    }
}