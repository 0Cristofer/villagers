using FluentAssertions;
using Villagers.GameServer.Domain.Commands;
using Xunit;

namespace Villagers.GameServer.Tests.Domain.Commands;

public class RegisterPlayerCommandTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var beforeTimestamp = DateTime.UtcNow;

        // Act
        var command = new RegisterPlayerCommand(playerId);
        var afterTimestamp = DateTime.UtcNow;

        // Assert
        command.PlayerId.Should().Be(playerId);
        command.Timestamp.Should().BeAfter(beforeTimestamp.AddMilliseconds(-1));
        command.Timestamp.Should().BeBefore(afterTimestamp.AddMilliseconds(1));
    }

    [Fact]
    public void Constructor_WithEmptyPlayerId_ShouldThrowArgumentException()
    {
        // Arrange
        var playerId = Guid.Empty;

        // Act & Assert
        var exception = Record.Exception(() => new RegisterPlayerCommand(playerId));
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Contain("Player ID cannot be empty");
    }

    [Fact]
    public async Task MultipleCommands_ShouldHaveDifferentTimestamps()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        var command1 = new RegisterPlayerCommand(playerId);
        await Task.Delay(1); // Ensure different timestamps
        var command2 = new RegisterPlayerCommand(playerId);

        // Assert
        command1.Timestamp.Should().BeBefore(command2.Timestamp);
    }
}