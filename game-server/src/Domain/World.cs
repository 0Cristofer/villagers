using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Domain.Commands.Requests;

namespace Villagers.GameServer.Domain;

public class World
{
    public delegate Task WorldTickHandler(World world);
    
    public Guid Id { get; }
    public WorldConfig Config { get; private set; }
    private long TickNumber { get; set; }
    public string Message { get; private set; } // Temporary test

    private readonly CommandQueue _commandQueue;
    private readonly object _tickLock = new();
    private bool _isRunning;

    public event WorldTickHandler? TickOccurredEvent;

    public World(WorldConfig config) 
        : this(Guid.NewGuid(), config, 0)
    {
    }

    public World(Guid id, WorldConfig config, long tickNumber)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("World ID cannot be empty", nameof(id));
        
        Id = id;
        Config = config ?? throw new ArgumentNullException(nameof(config));
        TickNumber = tickNumber;
        _commandQueue = new CommandQueue();
        _isRunning = false;
        Message = string.Empty;
    }

    public async Task Run(CancellationToken cancellationToken = default)
    {
        _isRunning = true;
        
        while (_isRunning && !cancellationToken.IsCancellationRequested)
        {
            Tick();

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

    public void Tick()
    {
        ProcessCommands();
        
        lock (_tickLock)
        {
            TickNumber++;
        }
    }

    private void ProcessCommands()
    {
        var commands = _commandQueue.GetCommandsAndClear();
        
        foreach (var command in commands)
        {
            try
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
                        throw new NotSupportedException($"Command type {command.GetType().Name} is not supported");
                }
                
                // Mark command as completed after successful processing
                command.MarkCompleted();
            }
            catch (Exception ex)
            {
                // Mark command as failed and rethrow - this is a critical failure
                command.MarkFailed(ex);
                throw;
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

    public ICommand EnqueueCommand(ICommandRequest request)
    {
        lock (_tickLock)
        {
            // Get next tick and create command atomically
            var nextTick = TickNumber + 1;
            var timeout = Config.TickInterval * 3; // Enough time to finish this tick and start the next one
            
            ICommand command = request switch
            {
                TestCommandRequest testRequest => new TestCommand(testRequest.PlayerId, testRequest.Message, nextTick, timeout),
                RegisterPlayerCommandRequest registerRequest => new RegisterPlayerCommand(registerRequest.PlayerId, registerRequest.StartingDirection, nextTick, timeout),
                _ => throw new NotSupportedException($"Command request type {request.GetType().Name} is not supported")
            };
            
            // Enqueue the command immediately while still holding the lock
            _commandQueue.EnqueueCommand(command);
            
            return command;
        }
    }

    // Keep this method for command replay during world restoration
    public void EnqueueExistingCommand(ICommand command)
    {
        lock (_tickLock)
        {
            var expectedTick = TickNumber + 1;
            if (command.TickNumber != expectedTick)
            {
                throw new InvalidOperationException(
                    $"Command tick number {command.TickNumber} does not match expected tick {expectedTick}. " +
                    $"Commands must be replayed in correct tick order.");
            }
            
            _commandQueue.EnqueueCommand(command);
        }
    }

    public long GetCurrentTickNumber()
    {
        lock (_tickLock)
        {
            return TickNumber;
        }
    }

    public void UpdateConfiguration(WorldConfig newConfig)
    {
        Config = newConfig ?? throw new ArgumentNullException(nameof(newConfig));
    }
}