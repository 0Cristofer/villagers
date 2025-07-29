namespace Villagers.GameServer.Domain.Commands;

public interface ICommand
{
    string PlayerId { get; }
    DateTime Timestamp { get; }
}