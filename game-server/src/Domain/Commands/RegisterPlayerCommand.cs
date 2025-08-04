namespace Villagers.GameServer.Domain.Commands;

public class RegisterPlayerCommand : ICommand
{
    public Guid PlayerId { get; }
    public DateTime Timestamp { get; }

    public RegisterPlayerCommand(Guid playerId)
    {
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player ID cannot be empty", nameof(playerId));
            
        PlayerId = playerId;
        Timestamp = DateTime.UtcNow;
    }
}