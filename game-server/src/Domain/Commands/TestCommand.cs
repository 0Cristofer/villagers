namespace Villagers.GameServer.Domain.Commands;

public class TestCommand : ICommand
{
    public Guid PlayerId { get; }
    public DateTime Timestamp { get; }
    public int TickNumber { get; }
    public string Message { get; }

    public TestCommand(Guid playerId, string message, int tickNumber)
    {
        PlayerId = playerId;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        TickNumber = tickNumber;
        Timestamp = DateTime.UtcNow;
    }
}