namespace Villagers.GameServer.Domain.Commands;

public class CompletedCommand : ICommand
{
    public Guid PlayerId => Guid.Empty;
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public long TickNumber => 0;

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