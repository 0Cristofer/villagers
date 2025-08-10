using Villagers.GameServer.Domain.Enums;

namespace Villagers.GameServer.Domain.Commands;

public class RegisterPlayerCommand : BaseCommand
{
    public override Guid PlayerId { get; }
    public StartingDirection StartingDirection { get; }
    public override DateTime Timestamp { get; }
    public override int TickNumber { get; }

    public RegisterPlayerCommand(Guid playerId, StartingDirection startingDirection, int tickNumber, TimeSpan timeout)
        : base(timeout)
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