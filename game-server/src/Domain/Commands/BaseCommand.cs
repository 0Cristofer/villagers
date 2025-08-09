namespace Villagers.GameServer.Domain.Commands;

public abstract class BaseCommand : ICommand
{
    public abstract Guid PlayerId { get; }
    public abstract DateTime Timestamp { get; }
    public abstract int TickNumber { get; }

    private readonly TaskCompletionSource<bool> _completion = new();
    private readonly CancellationTokenSource _timeoutCancellation = new();
    private readonly Task _delayTask;

    protected BaseCommand(TimeSpan timeout)
    {
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
    }

    public void MarkFailed(Exception exception)
    {
        _completion.SetException(exception);
        _timeoutCancellation.Cancel();
    }
}