using Villagers.GameServer.Domain.Enums;

namespace Villagers.GameServer.Domain.Commands;

public class RegisterPlayerCommand : BaseCommand
{
    public StartingDirection StartingDirection { get; }

    public RegisterPlayerCommand(Guid playerId, StartingDirection startingDirection, long tickNumber, TimeSpan timeout)
        : base(playerId, tickNumber, timeout)
    {
        if (!Enum.IsDefined(typeof(StartingDirection), startingDirection))
            throw new ArgumentException("Invalid starting direction", nameof(startingDirection));
            
        StartingDirection = startingDirection;
    }
}