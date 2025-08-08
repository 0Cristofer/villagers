using FluentAssertions;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;
using Xunit;

namespace Villagers.GameServer.Tests.Domain;

public class CommandQueueTests
{
    private readonly CommandQueue _commandQueue;

    public CommandQueueTests()
    {
        _commandQueue = new CommandQueue();
    }

    [Fact]
    public void EnqueueCommand_ShouldAddCommandToQueue()
    {
        // Arrange
        var command = new TestCommand(Guid.NewGuid(), "test message", 0);

        // Act
        _commandQueue.EnqueueCommand(command);
        var commands = _commandQueue.GetCommandsAndClear();

        // Assert
        commands.Should().HaveCount(1);
        commands[0].Should().Be(command);
    }

    [Fact]
    public void GetCommandsAndClear_ShouldReturnAllCommandsAndClearQueue()
    {
        // Arrange
        var command1 = new TestCommand(Guid.NewGuid(), "message1", 0);
        var command2 = new TestCommand(Guid.NewGuid(), "message2", 0);
        var command3 = new TestCommand(Guid.NewGuid(), "message3", 0);

        _commandQueue.EnqueueCommand(command1);
        _commandQueue.EnqueueCommand(command2);
        _commandQueue.EnqueueCommand(command3);

        // Act
        var commands = _commandQueue.GetCommandsAndClear();
        var secondCall = _commandQueue.GetCommandsAndClear();

        // Assert
        commands.Should().HaveCount(3);
        commands[0].Should().Be(command1);
        commands[1].Should().Be(command2);
        commands[2].Should().Be(command3);

        secondCall.Should().BeEmpty();
    }

    [Fact]
    public void GetCommandsAndClear_WithEmptyQueue_ShouldReturnEmptyList()
    {
        // Act
        var commands = _commandQueue.GetCommandsAndClear();

        // Assert
        commands.Should().BeEmpty();
    }

    [Fact]
    public void EnqueueCommand_ShouldBeThreadSafe()
    {
        // Arrange
        const int threadCount = 10;
        const int commandsPerThread = 100;
        var tasks = new Task[threadCount];

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            var threadId = i;
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < commandsPerThread; j++)
                {
                    _commandQueue.EnqueueCommand(new TestCommand(Guid.NewGuid(), $"message{j}", 0));
                }
            });
        }

        Task.WaitAll(tasks);
        var commands = _commandQueue.GetCommandsAndClear();

        // Assert
        commands.Should().HaveCount(threadCount * commandsPerThread);
    }

    [Fact]
    public void GetCommandsAndClear_ShouldMaintainFIFOOrder()
    {
        // Arrange
        var commands = new List<TestCommand>();
        for (int i = 0; i < 5; i++)
        {
            var command = new TestCommand(Guid.NewGuid(), $"message{i}", i);
            commands.Add(command);
            _commandQueue.EnqueueCommand(command);
        }

        // Act
        var retrievedCommands = _commandQueue.GetCommandsAndClear();

        // Assert
        retrievedCommands.Should().HaveCount(5);
        for (int i = 0; i < 5; i++)
        {
            retrievedCommands[i].Should().Be(commands[i]);
        }
    }
}