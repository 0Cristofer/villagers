using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Villagers.Api.Controllers;
using Villagers.Api.Data;
using Villagers.Api.Domain;
using Villagers.Api.Extensions;
using Villagers.Api.Models;
using Xunit;

namespace Villagers.Api.Tests.Controllers;

public class WorldsControllerTests : IDisposable
{
    private readonly ApiDbContext _context;
    private readonly WorldsController _controller;

    public WorldsControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApiDbContext(options);
        _controller = new WorldsController(_context);
    }

    [Fact]
    public async Task GetWorlds_WithNoWorlds_ShouldReturnEmptyList()
    {
        // Act
        var result = await _controller.GetWorlds();

        // Assert
        result.Should().BeOfType<ActionResult<IEnumerable<WorldResponse>>>();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var worlds = okResult!.Value as List<WorldResponse>;
        worlds.Should().NotBeNull();
        worlds!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWorlds_WithMultipleWorlds_ShouldReturnAllWorlds()
    {
        // Arrange
        var config1 = new WorldConfig("World One", TimeSpan.FromSeconds(5));
        var config2 = new WorldConfig("World Two", TimeSpan.FromSeconds(10));
        
        var world1 = new WorldRegistry(
            Guid.NewGuid(),
            "https://server1.example.com/gamehub",
            config1);
        
        var world2 = new WorldRegistry(
            Guid.NewGuid(),
            "https://server2.example.com/gamehub",
            config2);

        await _context.WorldRegistry.AddAsync(world1.ToEntity());
        await _context.WorldRegistry.AddAsync(world2.ToEntity());
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetWorlds();

        // Assert
        result.Should().BeOfType<ActionResult<IEnumerable<WorldResponse>>>();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var worlds = okResult!.Value as List<WorldResponse>;
        worlds.Should().NotBeNull();
        worlds!.Should().HaveCount(2);
        
        var worldNames = worlds.Select(w => w.Config.WorldName).ToList();
        worldNames.Should().Contain("World One");
        worldNames.Should().Contain("World Two");
    }

    [Fact]
    public async Task GetWorlds_ShouldReturnWorldsOrderedByRegistrationDate()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var config1 = new WorldConfig("World One", TimeSpan.FromSeconds(5));
        var config2 = new WorldConfig("World Two", TimeSpan.FromSeconds(10));
        var config3 = new WorldConfig("World Three", TimeSpan.FromSeconds(15));
        
        var world1 = new WorldRegistry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "https://server1.example.com/gamehub",
            config1,
            baseTime.AddMinutes(2)); // Registered second
        
        var world2 = new WorldRegistry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "https://server2.example.com/gamehub",
            config2,
            baseTime.AddMinutes(3)); // Registered third
        
        var world3 = new WorldRegistry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "https://server3.example.com/gamehub",
            config3,
            baseTime.AddMinutes(1)); // Registered first

        await _context.WorldRegistry.AddAsync(world1.ToEntity());
        await _context.WorldRegistry.AddAsync(world2.ToEntity());
        await _context.WorldRegistry.AddAsync(world3.ToEntity());
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetWorlds();

        // Assert
        var okResult = result.Result as OkObjectResult;
        var worlds = okResult!.Value as List<WorldResponse>;
        worlds.Should().NotBeNull();
        worlds!.Should().HaveCount(3);
        worlds[0].Config.WorldName.Should().Be("World Three"); // First registered
        worlds[1].Config.WorldName.Should().Be("World One");   // Second registered
        worlds[2].Config.WorldName.Should().Be("World Two");   // Third registered
    }

    [Fact]
    public async Task GetWorld_WithValidId_ShouldReturnWorld()
    {
        // Arrange
        var config = new WorldConfig("Test World", TimeSpan.FromSeconds(5));
        var world = new WorldRegistry(
            Guid.NewGuid(),
            "https://server.example.com/gamehub",
            config);

        var entity = world.ToEntity();
        await _context.WorldRegistry.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetWorld(world.WorldId);

        // Assert
        result.Should().BeOfType<ActionResult<WorldResponse>>();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var worldResponse = okResult!.Value as WorldResponse;
        worldResponse.Should().NotBeNull();
        worldResponse!.WorldId.Should().Be(world.WorldId);
        worldResponse.Config.WorldName.Should().Be("Test World");
        worldResponse.ServerEndpoint.Should().Be("https://server.example.com/gamehub");
        worldResponse.Config.TickInterval.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetWorld_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _controller.GetWorld(nonExistentId);

        // Assert
        result.Should().BeOfType<ActionResult<WorldResponse>>();
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetWorld_ShouldReturnWorldWithEmbeddedConfig()
    {
        // Arrange
        var worldName = "Test World with Config";
        var tickInterval = TimeSpan.FromSeconds(7);
        var config = new WorldConfig(worldName, tickInterval);
        var world = new WorldRegistry(
            Guid.NewGuid(),
            "https://server.example.com/gamehub",
            config);

        await _context.WorldRegistry.AddAsync(world.ToEntity());
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetWorld(world.WorldId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var worldResponse = okResult!.Value as WorldResponse;
        worldResponse.Should().NotBeNull();
        worldResponse!.Config.WorldName.Should().Be(worldName);
        worldResponse.Config.TickInterval.Should().Be(tickInterval);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}