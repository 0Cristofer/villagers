namespace Villagers.GameServer.Domain.Commands.Requests;

/// <summary>
/// Represents a request to execute a command in the game world.
/// Command requests are converted to actual commands by the world with proper tick assignment.
/// </summary>
public interface ICommandRequest
{
    Guid PlayerId { get; }
    DateTime Timestamp { get; }
    long? ProcessedTickNumber { get; }
    void SetProcessedTickNumber(long tickNumber);
}