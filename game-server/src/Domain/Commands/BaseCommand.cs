namespace Villagers.GameServer.Domain.Commands;

public abstract class BaseCommand : ICommand
{
    public Guid PlayerId { get; }
    public DateTime Timestamp { get; }
    public long TickNumber { get; }

    private readonly TaskCompletionSource<bool> _completion = new();
    private readonly CancellationTokenSource _timeoutCancellation = new();
    private readonly Task _delayTask;

    protected BaseCommand(Guid playerId, long tickNumber, TimeSpan timeout)
    {
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player ID cannot be empty", nameof(playerId));
            
        PlayerId = playerId;
        TickNumber = tickNumber;
        Timestamp = DateTime.UtcNow;
        
        // Set up timeout
        _delayTask = Task.Delay(timeout, _timeoutCancellation.Token);
    }

    public async Task WaitForCompletionAsync()
    {
        var completedTask = await Task.WhenAny(_completion.Task, _delayTask);
        
        if (completedTask == _delayTask)
            throw new TimeoutException();
    }

    public void MarkCompleted()
    {
        _completion.SetResult(true);
        _timeoutCancellation.Cancel();
        _timeoutCancellation.Dispose();
    }

    public void MarkFailed(Exception exception)
    {
        _completion.SetException(exception);
        _timeoutCancellation.Cancel();
        _timeoutCancellation.Dispose();
    }
}