using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Villagers.Api.Data;
using Villagers.Api.Domain;
using Villagers.Api.Extensions;
using Villagers.Api.Models;
using Villagers.Api.Services;
using Xunit;

namespace Villagers.Api.Tests.Services;

public class WorldRegistryServiceTests : IDisposable
{
    private readonly ApiDbContext _context;
    private readonly WorldRegistryService _service;

    public WorldRegistryServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApiDbContext(options);
        _service = new WorldRegistryService(_context);
    }

    [Fact]
    public async Task RegisterWorldAsync_WithValidRequest_ShouldCreateWorldRegistry()
    {
        // Arrange
        var request = new RegisterWorldRequest
        {
            WorldId = Guid.NewGuid(),
            ServerEndpoint = "https://localhost:5034/gamehub",
            Config = new WorldConfigModel
            {
                WorldName = "Test World",
                TickInterval = TimeSpan.FromSeconds(5)
            }
        };

        // Act
        var result = await _service.RegisterWorldAsync(request);

        // Assert
        result.Should().NotBe(Guid.Empty);
        
        var entity = await _context.WorldRegistry.FirstAsync();
        entity.WorldId.Should().Be(request.WorldId);
        entity.ServerEndpoint.Should().Be(request.ServerEndpoint);
        entity.Config.WorldName.Should().Be(request.Config.WorldName);
        entity.Config.TickInterval.Should().Be(request.Config.TickInterval);
    }

    [Fact]
    public async Task UnregisterWorldAsync_WithExistingWorld_ShouldRemoveWorldAndReturnTrue()
    {
        // Arrange
        var worldId = Guid.NewGuid();
        var request = new RegisterWorldRequest
        {
            WorldId = worldId,
            ServerEndpoint = "https://localhost:5034/gamehub",
            Config = new WorldConfigModel
            {
                WorldName = "Test World",
                TickInterval = TimeSpan.FromSeconds(5)
            }
        };

        await _service.RegisterWorldAsync(request);

        // Act
        var result = await _service.UnregisterWorldAsync(worldId);

        // Assert
        result.Should().BeTrue();
        
        var entity = await _context.WorldRegistry.FirstOrDefaultAsync(w => w.WorldId == worldId);
        entity.Should().BeNull();
    }

    [Fact]
    public async Task UnregisterWorldAsync_WithNonExistentWorld_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentWorldId = Guid.NewGuid();

        // Act
        var result = await _service.UnregisterWorldAsync(nonExistentWorldId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllWorldsAsync_WithNoWorlds_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GetAllWorldsAsync();

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetAllWorldsAsync_WithWorlds_ShouldReturnOrderedWorlds()
    {
        // Arrange
        var world1 = new RegisterWorldRequest
        {
            WorldId = Guid.NewGuid(),
            ServerEndpoint = "https://localhost:5034/gamehub",
            Config = new WorldConfigModel
            {
                WorldName = "World 1",
                TickInterval = TimeSpan.FromSeconds(5)
            }
        };

        var world2 = new RegisterWorldRequest
        {
            WorldId = Guid.NewGuid(),
            ServerEndpoint = "https://localhost:5035/gamehub",
            Config = new WorldConfigModel
            {
                WorldName = "World 2",
                TickInterval = TimeSpan.FromSeconds(10)
            }
        };

        await _service.RegisterWorldAsync(world1);
        await Task.Delay(1); // Ensure different timestamps
        await _service.RegisterWorldAsync(world2);

        // Act
        var result = await _service.GetAllWorldsAsync();

        // Assert
        result.Should().HaveCount(2);
        var worldsList = result.ToList();
        worldsList[0].Config.WorldName.Should().Be("World 1");
        worldsList[1].Config.WorldName.Should().Be("World 2");
    }

    [Fact]
    public async Task GetWorldAsync_WithExistingWorld_ShouldReturnWorld()
    {
        // Arrange
        var worldId = Guid.NewGuid();
        var request = new RegisterWorldRequest
        {
            WorldId = worldId,
            ServerEndpoint = "https://localhost:5034/gamehub",
            Config = new WorldConfigModel
            {
                WorldName = "Test World",
                TickInterval = TimeSpan.FromSeconds(5)
            }
        };

        await _service.RegisterWorldAsync(request);

        // Act
        var result = await _service.GetWorldAsync(worldId);

        // Assert
        result.Should().NotBeNull();
        result!.WorldId.Should().Be(worldId);
        result.Config.WorldName.Should().Be("Test World");
        result.ServerEndpoint.Should().Be("https://localhost:5034/gamehub");
    }

    [Fact]
    public async Task GetWorldAsync_WithNonExistentWorld_ShouldReturnNull()
    {
        // Arrange
        var nonExistentWorldId = Guid.NewGuid();

        // Act
        var result = await _service.GetWorldAsync(nonExistentWorldId);

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}