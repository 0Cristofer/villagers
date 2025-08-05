using FluentAssertions;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Domain.Enums;
using Xunit;

namespace Villagers.GameServer.Tests.Domain.Commands;

public class RegisterPlayerCommandTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var startingDirection = StartingDirection.North;
        var beforeTimestamp = DateTime.UtcNow;

        // Act
        var command = new RegisterPlayerCommand(playerId, startingDirection);
        var afterTimestamp = DateTime.UtcNow;

        // Assert
        command.PlayerId.Should().Be(playerId);
        command.StartingDirection.Should().Be(startingDirection);
        command.Timestamp.Should().BeAfter(beforeTimestamp.AddMilliseconds(-1));
        command.Timestamp.Should().BeBefore(afterTimestamp.AddMilliseconds(1));
    }

    [Fact]
    public void Constructor_WithEmptyPlayerId_ShouldThrowArgumentException()
    {
        // Arrange
        var playerId = Guid.Empty;
        var startingDirection = StartingDirection.North;

        // Act & Assert
        var exception = Record.Exception(() => new RegisterPlayerCommand(playerId, startingDirection));
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Contain("Player ID cannot be empty");
    }

    [Fact]
    public async Task MultipleCommands_ShouldHaveDifferentTimestamps()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var startingDirection = StartingDirection.East;

        // Act
        var command1 = new RegisterPlayerCommand(playerId, startingDirection);
        await Task.Delay(1); // Ensure different timestamps
        var command2 = new RegisterPlayerCommand(playerId, startingDirection);

        // Assert
        command1.Timestamp.Should().BeBefore(command2.Timestamp);
    }

    [Fact]
    public void Constructor_WithInvalidStartingDirection_ShouldThrowArgumentException()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var invalidDirection = (StartingDirection)999;

        // Act & Assert
        var exception = Record.Exception(() => new RegisterPlayerCommand(playerId, invalidDirection));
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Contain("Invalid starting direction");
    }

    [Theory]
    [InlineData(StartingDirection.North)]
    [InlineData(StartingDirection.South)]
    [InlineData(StartingDirection.East)]
    [InlineData(StartingDirection.West)]
    [InlineData(StartingDirection.Northeast)]
    [InlineData(StartingDirection.Northwest)]
    [InlineData(StartingDirection.Southeast)]
    [InlineData(StartingDirection.Southwest)]
    [InlineData(StartingDirection.Random)]
    public void Constructor_WithValidStartingDirection_ShouldSucceed(StartingDirection direction)
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        var command = new RegisterPlayerCommand(playerId, direction);

        // Assert
        command.StartingDirection.Should().Be(direction);
        command.PlayerId.Should().Be(playerId);
    }
}