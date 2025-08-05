using FluentAssertions;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Domain.Enums;
using Xunit;

namespace Villagers.GameServer.Tests.Domain;

public class WorldTests
{
    private readonly CommandQueue _commandQueue;
    private readonly WorldConfig _worldConfig;
    private readonly World _world;

    public WorldTests()
    {
        _commandQueue = new CommandQueue();
        _worldConfig = new WorldConfig("Test World", TimeSpan.FromMilliseconds(10));
        _world = new World(_worldConfig, _commandQueue);
    }

    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Assert
        _world.Config.WorldName.Should().Be("Test World");
        _world.GetCurrentTickNumber().Should().Be(0);
        _world.Message.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullConfig_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new World(null!, _commandQueue);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("config");
    }

    [Fact]
    public void Constructor_WithNullCommandQueue_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new World(_worldConfig, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("commandQueue");
    }

    [Fact]
    public async Task Run_ShouldIncrementTickNumber()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        var initialTickNumber = _world.GetCurrentTickNumber();

        // Act
        await _world.Run(cts.Token);

        // Assert
        _world.GetCurrentTickNumber().Should().BeGreaterThan(initialTickNumber);
    }

    [Fact]
    public async Task Run_ShouldFireTickOccurredEvent()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        var tickCount = 0;
        _world.TickOccurredEvent += async (world) =>
        {
            tickCount++;
            await Task.CompletedTask;
        };

        // Act
        await _world.Run(cts.Token);

        // Assert
        tickCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Run_WithTestCommand_ShouldProcessCommand()
    {
        // Arrange
        var testCommand = new TestCommand(Guid.NewGuid(), "Hello World", 0);
        _commandQueue.EnqueueCommand(testCommand);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        // Act
        await _world.Run(cts.Token);

        // Assert
        _world.Message.Should().Be("Hello World");
    }

    [Fact]
    public async Task Stop_ShouldStopWorldExecution()
    {
        // Arrange
        var task = _world.Run();
        await Task.Delay(50); // Let it start

        // Act
        _world.Stop();
        await task; // Should complete soon after Stop is called

        // Assert
        task.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Run_WithMultipleCommands_ShouldProcessAllCommands()
    {
        // Arrange
        var commands = new[]
        {
            new TestCommand(Guid.NewGuid(), "First", 0),
            new TestCommand(Guid.NewGuid(), "Second", 1),
            new TestCommand(Guid.NewGuid(), "Third", 2)
        };

        foreach (var cmd in commands)
        {
            _commandQueue.EnqueueCommand(cmd);
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        // Act
        await _world.Run(cts.Token);

        // Assert
        _world.Message.Should().Be("Third"); // Last command processed
    }

    [Fact]
    public async Task Run_WithRegisterPlayerCommand_ShouldProcessCommand()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var registerCommand = new RegisterPlayerCommand(playerId, StartingDirection.North, 0);
        _commandQueue.EnqueueCommand(registerCommand);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        // Act
        await _world.Run(cts.Token);

        // Assert - Command should be processed without throwing
        // (We don't have observable state changes yet, but command processing shouldn't crash)
        _world.GetCurrentTickNumber().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Run_WithMixedCommands_ShouldProcessAllCommands()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var testCommand = new TestCommand(playerId, "Test Message", 0);
        var registerCommand = new RegisterPlayerCommand(playerId, StartingDirection.North, 1);
        
        _commandQueue.EnqueueCommand(testCommand);
        _commandQueue.EnqueueCommand(registerCommand);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        // Act
        await _world.Run(cts.Token);

        // Assert
        _world.Message.Should().Be("Test Message");
        _world.GetCurrentTickNumber().Should().BeGreaterThan(0);
    }
}