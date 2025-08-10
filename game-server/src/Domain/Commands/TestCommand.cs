namespace Villagers.GameServer.Domain.Commands;

public class TestCommand : BaseCommand
{
    public string Message { get; }

    public TestCommand(Guid playerId, string message, long tickNumber, TimeSpan timeout)
        : base(playerId, tickNumber, timeout)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }
}