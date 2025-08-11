using Villagers.GameServer.Domain.Enums;

namespace Villagers.GameServer.Domain.Commands.Requests;

public class RegisterPlayerCommandRequest : BaseCommandRequest
{
    public StartingDirection StartingDirection { get; }

    public RegisterPlayerCommandRequest(Guid playerId, StartingDirection startingDirection)
        : base(playerId)
    {
        if (!Enum.IsDefined(typeof(StartingDirection), startingDirection))
            throw new ArgumentException("Invalid starting direction", nameof(startingDirection));
            
        StartingDirection = startingDirection;
    }
}