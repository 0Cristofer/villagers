using Villagers.GameServer.Domain.Enums;

namespace Villagers.GameServer.Domain.Commands.Requests;

public class RegisterPlayerCommandRequest : ICommandRequest
{
    public Guid PlayerId { get; }
    public DateTime Timestamp { get; }
    public StartingDirection StartingDirection { get; }

    public RegisterPlayerCommandRequest(Guid playerId, StartingDirection startingDirection)
    {
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player ID cannot be empty", nameof(playerId));
            
        PlayerId = playerId;
        StartingDirection = startingDirection;
        Timestamp = DateTime.UtcNow;
    }
}