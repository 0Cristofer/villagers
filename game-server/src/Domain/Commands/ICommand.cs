namespace Villagers.GameServer.Domain.Commands;

public interface ICommand
{
    Guid PlayerId { get; }
    DateTime Timestamp { get; }
}