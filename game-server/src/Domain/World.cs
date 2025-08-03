using Villagers.GameServer.Domain.Commands;

namespace Villagers.GameServer.Domain;

public class World
{
    public delegate Task WorldTickHandler(World world);
    
    public string Name { get; private set; }
    public int TickNumber { get; private set; }
    public string Message { get; private set; } // Temporary test

    private readonly TimeSpan _tickInterval;
    private readonly CommandQueue _commandQueue;
    private bool _isRunning;

    public event WorldTickHandler? TickOccurredEvent;

    public World(string name, TimeSpan tickInterval, CommandQueue commandQueue) 
        : this(name, tickInterval, commandQueue, 0)
    {
    }

    public World(string name, TimeSpan tickInterval, CommandQueue commandQueue, int tickNumber)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        TickNumber = tickNumber;
        _tickInterval = tickInterval;
        _commandQueue = commandQueue ?? throw new ArgumentNullException(nameof(commandQueue));
        _isRunning = false;
        Message = string.Empty;
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
        ProcessCommands();
        TickNumber++;
    }

    private void ProcessCommands()
    {
        var commands = _commandQueue.GetCommandsAndClear();
        
        foreach (var command in commands)
        {
            switch (command)
            {
                case TestCommand testCmd:
                    ProcessTestCommand(testCmd);
                    break;
                default:
                    Console.WriteLine($"Unknown command type: {command.GetType().Name} from player {command.PlayerId}");
                    break;
            }
        }
    }

    private void ProcessTestCommand(TestCommand command)
    {
        Console.WriteLine($"Processing test command from player {command.PlayerId}: {command.Message} at {command.Timestamp}");
        Message = command.Message; // Temporary test
    }

    public void Stop()
    {
        _isRunning = false;
    }
}