namespace Villagers.GameServer.Domain.Commands;

public class CompletedCommand : ICommand
{
    public Guid PlayerId { get; }
    public DateTime Timestamp { get; }
    public int TickNumber { get; }
    
    public CompletedCommand()
    {
        PlayerId = Guid.Empty;
        Timestamp = DateTime.UtcNow;
        TickNumber = 0;
    }

    public Task WaitForCompletionAsync()
    {
        // Already completed, return immediately
        return Task.CompletedTask;
    }

    public void MarkCompleted()
    {
        // Already completed, no-op
    }

    public void MarkFailed(Exception exception)
    {
        // Already completed, no-op
        // Could log a warning if needed
    }
}