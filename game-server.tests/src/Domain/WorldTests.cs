using FluentAssertions;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;
using Xunit;

namespace Villagers.GameServer.Tests.Domain;

public class WorldTests
{
    private readonly CommandQueue _commandQueue;
    private readonly World _world;

    public WorldTests()
    {
        _commandQueue = new CommandQueue();
        _world = new World("Test World", TimeSpan.FromMilliseconds(10), _commandQueue);
    }

    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Assert
        _world.Name.Should().Be("Test World");
        _world.TickNumber.Should().Be(0);
        _world.Message.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new World(null!, TimeSpan.FromSeconds(1), _commandQueue);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Constructor_WithNullCommandQueue_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new World("Test", TimeSpan.FromSeconds(1), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("commandQueue");
    }

    [Fact]
    public async Task Run_ShouldIncrementTickNumber()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        var initialTickNumber = _world.TickNumber;

        // Act
        await _world.Run(cts.Token);

        // Assert
        _world.TickNumber.Should().BeGreaterThan(initialTickNumber);
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
        var testCommand = new TestCommand(Guid.NewGuid(), "Hello World");
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
            new TestCommand(Guid.NewGuid(), "First"),
            new TestCommand(Guid.NewGuid(), "Second"),
            new TestCommand(Guid.NewGuid(), "Third")
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
}