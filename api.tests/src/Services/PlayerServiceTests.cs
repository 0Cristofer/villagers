using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Villagers.Api.Data;
using Villagers.Api.Domain;
using Villagers.Api.Entities;
using Villagers.Api.Services;
using Xunit;

namespace Villagers.Api.Tests.Services;

public class PlayerServiceTests : IDisposable
{
    private readonly ApiDbContext _context;
    private readonly Mock<IWorldRegistryService> _worldRegistryServiceMock;
    private readonly PlayerService _service;

    public PlayerServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApiDbContext(options);
        _worldRegistryServiceMock = new Mock<IWorldRegistryService>();
        _service = new PlayerService(_context, _worldRegistryServiceMock.Object);
    }

    [Fact]
    public async Task RegisterPlayerForWorldAsync_WithExistingPlayer_ShouldUpdateRegisteredWorlds()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var worldId = Guid.NewGuid();
        
        // Mock world registry service to return a valid world
        var mockWorld = new WorldRegistry(
            Guid.NewGuid(),
            worldId,
            "https://localhost:5034/gamehub",
            new WorldConfig("Test World", TimeSpan.FromSeconds(5)),
            DateTime.UtcNow
        );
        _worldRegistryServiceMock.Setup(x => x.GetWorldAsync(worldId))
            .ReturnsAsync(mockWorld);
        
        var player = new PlayerEntity
        {
            Id = playerId,
            UserName = "testuser",
            Email = "test@example.com",
            RegisteredWorldIds = new List<Guid>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(player);
        await _context.SaveChangesAsync();

        // Act
        await _service.RegisterPlayerForWorldAsync(playerId, worldId);

        // Assert
        var updatedPlayer = await _context.Users.FirstAsync(p => p.Id == playerId);
        updatedPlayer.RegisteredWorldIds.Should().Contain(worldId);
        updatedPlayer.UpdatedAt.Should().BeOnOrAfter(player.UpdatedAt);
    }

    [Fact]
    public async Task RegisterPlayerForWorldAsync_WithAlreadyRegisteredWorld_ShouldNotDuplicate()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var worldId = Guid.NewGuid();
        
        // Mock world registry service to return a valid world
        var mockWorld = new WorldRegistry(
            Guid.NewGuid(),
            worldId,
            "https://localhost:5034/gamehub",
            new WorldConfig("Test World", TimeSpan.FromSeconds(5)),
            DateTime.UtcNow
        );
        _worldRegistryServiceMock.Setup(x => x.GetWorldAsync(worldId))
            .ReturnsAsync(mockWorld);
        
        var player = new PlayerEntity
        {
            Id = playerId,
            UserName = "testuser",
            Email = "test@example.com",
            RegisteredWorldIds = new List<Guid> { worldId },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(player);
        await _context.SaveChangesAsync();

        // Act
        await _service.RegisterPlayerForWorldAsync(playerId, worldId);

        // Assert
        var updatedPlayer = await _context.Users.FirstAsync(p => p.Id == playerId);
        updatedPlayer.RegisteredWorldIds.Should().HaveCount(1);
        updatedPlayer.RegisteredWorldIds.Should().Contain(worldId);
    }

    [Fact]
    public async Task RegisterPlayerForWorldAsync_WithMultipleWorlds_ShouldAddToList()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var existingWorldId = Guid.NewGuid();
        var newWorldId = Guid.NewGuid();
        
        // Mock world registry service to return a valid world for the new world
        var mockWorld = new WorldRegistry(
            Guid.NewGuid(),
            newWorldId,
            "https://localhost:5035/gamehub",
            new WorldConfig("New World", TimeSpan.FromSeconds(10)),
            DateTime.UtcNow
        );
        _worldRegistryServiceMock.Setup(x => x.GetWorldAsync(newWorldId))
            .ReturnsAsync(mockWorld);
        
        var player = new PlayerEntity
        {
            Id = playerId,
            UserName = "testuser",
            Email = "test@example.com",
            RegisteredWorldIds = new List<Guid> { existingWorldId },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(player);
        await _context.SaveChangesAsync();

        // Act
        await _service.RegisterPlayerForWorldAsync(playerId, newWorldId);

        // Assert
        var updatedPlayer = await _context.Users.FirstAsync(p => p.Id == playerId);
        updatedPlayer.RegisteredWorldIds.Should().HaveCount(2);
        updatedPlayer.RegisteredWorldIds.Should().Contain(existingWorldId);
        updatedPlayer.RegisteredWorldIds.Should().Contain(newWorldId);
    }

    [Fact]
    public async Task RegisterPlayerForWorldAsync_WithNonExistentPlayer_ShouldThrowException()
    {
        // Arrange
        var nonExistentPlayerId = Guid.NewGuid();
        var worldId = Guid.NewGuid();

        // Mock world registry service to return a valid world so we get to the player validation
        var mockWorld = new WorldRegistry(
            Guid.NewGuid(),
            worldId,
            "https://localhost:5034/gamehub",
            new WorldConfig("Test World", TimeSpan.FromSeconds(5)),
            DateTime.UtcNow
        );
        _worldRegistryServiceMock.Setup(x => x.GetWorldAsync(worldId))
            .ReturnsAsync(mockWorld);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => 
            _service.RegisterPlayerForWorldAsync(nonExistentPlayerId, worldId));

        exception.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Contain($"Player with ID {nonExistentPlayerId} not found");
    }

    [Fact]
    public async Task RegisterPlayerForWorldAsync_WithNonExistentWorld_ShouldThrowException()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var nonExistentWorldId = Guid.NewGuid();

        // Mock world registry service to return null (world doesn't exist)
        _worldRegistryServiceMock.Setup(x => x.GetWorldAsync(nonExistentWorldId))
            .ReturnsAsync((WorldRegistry?)null);

        var player = new PlayerEntity
        {
            Id = playerId,
            UserName = "testuser",
            Email = "test@example.com",
            RegisteredWorldIds = new List<Guid>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(player);
        await _context.SaveChangesAsync();

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => 
            _service.RegisterPlayerForWorldAsync(playerId, nonExistentWorldId));

        exception.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Contain($"World with ID {nonExistentWorldId} not found in registry");
    }

    [Fact]
    public async Task RegisterPlayerForWorldAsync_ShouldPersistChanges()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var worldId = Guid.NewGuid();
        var databaseName = Guid.NewGuid().ToString();
        
        // Use a shared database name for both contexts
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        using var setupContext = new ApiDbContext(options);
        var player = new PlayerEntity
        {
            Id = playerId,
            UserName = "testuser",
            Email = "test@example.com",
            RegisteredWorldIds = new List<Guid>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        setupContext.Users.Add(player);
        await setupContext.SaveChangesAsync();

        // Act
        using var serviceContext = new ApiDbContext(options);
        var mockWorldService = new Mock<IWorldRegistryService>();
        var mockWorld = new WorldRegistry(
            Guid.NewGuid(),
            worldId,
            "https://localhost:5034/gamehub",
            new WorldConfig("Test World", TimeSpan.FromSeconds(5)),
            DateTime.UtcNow
        );
        mockWorldService.Setup(x => x.GetWorldAsync(worldId)).ReturnsAsync(mockWorld);
        
        var service = new PlayerService(serviceContext, mockWorldService.Object);
        await service.RegisterPlayerForWorldAsync(playerId, worldId);

        // Assert - Create a new context to verify persistence
        using var verificationContext = new ApiDbContext(options);
        var persistedPlayer = await verificationContext.Users.FirstAsync(p => p.Id == playerId);
        persistedPlayer.RegisteredWorldIds.Should().Contain(worldId);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}