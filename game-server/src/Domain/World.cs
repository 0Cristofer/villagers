using Villagers.GameServer.Domain.Commands;

namespace Villagers.GameServer.Domain;

public class World
{
    public delegate Task WorldTickHandler(World world);
    
    public Guid Id { get; private set; }
    public WorldConfig Config { get; private set; }
    public int TickNumber { get; private set; }
    public string Message { get; private set; } // Temporary test

    private readonly CommandQueue _commandQueue;
    private bool _isRunning;

    public event WorldTickHandler? TickOccurredEvent;

    public World(WorldConfig config, CommandQueue commandQueue) 
        : this(Guid.NewGuid(), config, commandQueue, 0)
    {
    }

    public World(Guid id, WorldConfig config, CommandQueue commandQueue, int tickNumber)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("World ID cannot be empty", nameof(id));
        
        Id = id;
        Config = config ?? throw new ArgumentNullException(nameof(config));
        TickNumber = tickNumber;
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
                await Task.Delay(Config.TickInterval, cancellationToken);
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
                case RegisterPlayerCommand registerCmd:
                    ProcessRegisterPlayerCommand(registerCmd);
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

    private void ProcessRegisterPlayerCommand(RegisterPlayerCommand command)
    {
        Console.WriteLine($"Processing player registration: Player {command.PlayerId} registering for world {Id} at {command.Timestamp}");
        // TODO: Add player to world's registered players list
        // For now, just log the registration
    }

    public void Stop()
    {
        _isRunning = false;
    }
}