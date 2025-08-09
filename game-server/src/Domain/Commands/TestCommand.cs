namespace Villagers.GameServer.Domain.Commands;

public class TestCommand : BaseCommand
{
    public override Guid PlayerId { get; }
    public override DateTime Timestamp { get; }
    public override int TickNumber { get; }
    public string Message { get; }

    public TestCommand(Guid playerId, string message, int tickNumber, TimeSpan timeout)
        : base(timeout)
    {
        PlayerId = playerId;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        TickNumber = tickNumber;
        Timestamp = DateTime.UtcNow;
    }
}