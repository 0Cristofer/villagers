namespace Villagers.GameServer.Domain;

public class World
{
    public delegate Task WorldTickHandler(World world);
    
    public string Name { get; private set; }
    public int TickNumber { get; private set; }

    private readonly TimeSpan _tickInterval;
    private bool _isRunning;

    public event WorldTickHandler? TickOccurredEvent;

    public World(string name, TimeSpan tickInterval)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        TickNumber = 0;
        _tickInterval = tickInterval;
        _isRunning = false;
    }

    public async Task Run(CancellationToken cancellationToken = default)
    {
        _isRunning = true;
        
        while (_isRunning && !cancellationToken.IsCancellationRequested)
        {
            Tick();
            
            // Fire tick event to notify subscribers
            await (TickOccurredEvent?.Invoke(this) ?? Task.CompletedTask);
            
            try
            {
                await Task.Delay(_tickInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
        
        _isRunning = false;
    }

    private void Tick()
    {
        TickNumber++;
    }

    public void Stop()
    {
        _isRunning = false;
    }
}