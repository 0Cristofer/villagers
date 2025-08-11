namespace Villagers.GameServer.Domain.Commands.Requests;

public class TestCommandRequest : BaseCommandRequest
{
    public string Message { get; }

    public TestCommandRequest(Guid playerId, string message)
        : base(playerId)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }
}