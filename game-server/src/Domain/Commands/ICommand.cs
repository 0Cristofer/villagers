namespace Villagers.GameServer.Domain.Commands;

public interface ICommand
{
    Guid PlayerId { get; }
    DateTime Timestamp { get; }
    long TickNumber { get; }
    
    Task WaitForCompletionAsync();
    void MarkCompleted();
    void MarkFailed(Exception exception);
}