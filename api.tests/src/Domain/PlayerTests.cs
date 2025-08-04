using FluentAssertions;
using Villagers.Api.Domain;
using Xunit;

namespace Villagers.Api.Tests.Domain;

public class PlayerTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreatePlayer()
    {
        // Arrange
        var id = Guid.NewGuid();
        var username = "testuser";

        // Act
        var player = new Player(id, username);

        // Assert
        player.Id.Should().Be(id);
        player.Username.Should().Be(username);
        player.RegisteredWorldIds.Should().BeEmpty();
        player.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        player.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange
        var id = Guid.Empty;
        var username = "testuser";

        // Act & Assert
        var act = () => new Player(id, username);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Player ID cannot be empty*")
           .And.ParamName.Should().Be("id");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidUsername_ShouldThrowArgumentException(string username)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        var act = () => new Player(id, username);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Username cannot be empty*");
    }

    [Fact]
    public void Constructor_WithFullParameters_ShouldCreatePlayerWithProvidedValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var username = "testuser";
        var worldIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow.AddHours(-1);

        // Act
        var player = new Player(id, username, worldIds, createdAt, updatedAt);

        // Assert
        player.Id.Should().Be(id);
        player.Username.Should().Be(username);
        player.RegisteredWorldIds.Should().BeEquivalentTo(worldIds);
        player.CreatedAt.Should().Be(createdAt);
        player.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void RegisterForWorld_WithNewWorldId_ShouldAddToList()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "testuser");
        var worldId = Guid.NewGuid();

        // Act
        player.RegisterForWorld(worldId);

        // Assert
        player.RegisteredWorldIds.Should().Contain(worldId);
        player.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RegisterForWorld_WithExistingWorldId_ShouldNotAddDuplicate()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "testuser");
        var worldId = Guid.NewGuid();
        player.RegisterForWorld(worldId);
        var initialUpdateTime = player.UpdatedAt;

        // Wait a bit to ensure time difference
        Thread.Sleep(10);

        // Act
        player.RegisterForWorld(worldId);

        // Assert
        player.RegisteredWorldIds.Should().ContainSingle().Which.Should().Be(worldId);
        player.UpdatedAt.Should().Be(initialUpdateTime); // Should not update when no change
    }

    [Fact]
    public void UnregisterFromWorld_WithExistingWorldId_ShouldRemoveFromList()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "testuser");
        var worldId = Guid.NewGuid();
        player.RegisterForWorld(worldId);

        // Act
        player.UnregisterFromWorld(worldId);

        // Assert
        player.RegisteredWorldIds.Should().BeEmpty();
        player.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UnregisterFromWorld_WithNonExistingWorldId_ShouldNotChangeList()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "testuser");
        var initialUpdateTime = player.UpdatedAt;
        var worldId = Guid.NewGuid();

        // Wait a bit to ensure time difference
        Thread.Sleep(10);

        // Act
        player.UnregisterFromWorld(worldId);

        // Assert
        player.RegisteredWorldIds.Should().BeEmpty();
        player.UpdatedAt.Should().Be(initialUpdateTime); // Should not update when no change
    }

    [Fact]
    public void IsRegisteredForWorld_WithRegisteredWorld_ShouldReturnTrue()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "testuser");
        var worldId = Guid.NewGuid();
        player.RegisterForWorld(worldId);

        // Act
        var result = player.IsRegisteredForWorld(worldId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRegisteredForWorld_WithUnregisteredWorld_ShouldReturnFalse()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "testuser");
        var worldId = Guid.NewGuid();

        // Act
        var result = player.IsRegisteredForWorld(worldId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RegisterForWorld_MultipleWorlds_ShouldMaintainAllWorlds()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "testuser");
        var worldIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        foreach (var worldId in worldIds)
        {
            player.RegisterForWorld(worldId);
        }

        // Assert
        player.RegisteredWorldIds.Should().BeEquivalentTo(worldIds);
        player.RegisteredWorldIds.Should().HaveCount(5);
    }
}