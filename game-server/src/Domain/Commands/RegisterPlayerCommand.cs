using Villagers.GameServer.Domain.Enums;

namespace Villagers.GameServer.Domain.Commands;

public class RegisterPlayerCommand : ICommand
{
    public Guid PlayerId { get; }
    public StartingDirection StartingDirection { get; }
    public DateTime Timestamp { get; }
    public int TickNumber { get; }

    public RegisterPlayerCommand(Guid playerId, StartingDirection startingDirection, int tickNumber)
    {
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player ID cannot be empty", nameof(playerId));
            
        if (!Enum.IsDefined(typeof(StartingDirection), startingDirection))
            throw new ArgumentException("Invalid starting direction", nameof(startingDirection));
            
        PlayerId = playerId;
        StartingDirection = startingDirection;
        TickNumber = tickNumber;
        Timestamp = DateTime.UtcNow;
    }
}