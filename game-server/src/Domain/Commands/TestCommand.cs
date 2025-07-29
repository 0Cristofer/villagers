namespace Villagers.GameServer.Domain.Commands;

public class TestCommand : ICommand
{
    public string PlayerId { get; }
    public DateTime Timestamp { get; }
    public string Message { get; }

    public TestCommand(string playerId, string message)
    {
        PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Timestamp = DateTime.UtcNow;
    }
}