using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Villagers.Api.Controllers;
using Villagers.Api.Domain;
using Villagers.Api.Models;
using Villagers.Api.Services;
using Xunit;

namespace Villagers.Api.Tests.Controllers;

public class WorldsControllerTests
{
    private readonly Mock<IWorldRegistryService> _worldRegistryServiceMock;
    private readonly WorldsController _controller;

    public WorldsControllerTests()
    {
        _worldRegistryServiceMock = new Mock<IWorldRegistryService>();
        _controller = new WorldsController(_worldRegistryServiceMock.Object);
    }

    [Fact]
    public async Task GetWorlds_WithNoWorlds_ShouldReturnEmptyList()
    {
        // Arrange
        _worldRegistryServiceMock.Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(new List<WorldRegistry>());

        // Act
        var result = await _controller.GetWorlds();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        var worlds = okResult.Value as IEnumerable<WorldResponse>;
        worlds.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetWorlds_WithWorlds_ShouldReturnWorldList()
    {
        // Arrange
        var worldRegistries = new List<WorldRegistry>
        {
            new WorldRegistry(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "https://localhost:5034/gamehub",
                new WorldConfig("Test World 1", TimeSpan.FromSeconds(5)),
                DateTime.UtcNow
            ),
            new WorldRegistry(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "https://localhost:5035/gamehub",
                new WorldConfig("Test World 2", TimeSpan.FromSeconds(10)),
                DateTime.UtcNow
            )
        };

        _worldRegistryServiceMock.Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worldRegistries);

        // Act
        var result = await _controller.GetWorlds();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        var worlds = okResult.Value as IEnumerable<WorldResponse>;
        worlds.Should().NotBeNull().And.HaveCount(2);
    }

    [Fact]
    public async Task GetWorld_WithExistingWorld_ShouldReturnWorld()
    {
        // Arrange
        var worldId = Guid.NewGuid();
        var worldRegistry = new WorldRegistry(
            Guid.NewGuid(),
            worldId,
            "https://localhost:5034/gamehub",
            new WorldConfig("Test World", TimeSpan.FromSeconds(5)),
            DateTime.UtcNow
        );

        _worldRegistryServiceMock.Setup(x => x.GetWorldAsync(worldId))
            .ReturnsAsync(worldRegistry);

        // Act
        var result = await _controller.GetWorld(worldId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        var world = okResult.Value as WorldResponse;
        world.Should().NotBeNull();
        world!.WorldId.Should().Be(worldId.ToString());
    }

    [Fact]
    public async Task GetWorld_WithNonExistentWorld_ShouldReturnNotFound()
    {
        // Arrange
        var worldId = Guid.NewGuid();
        _worldRegistryServiceMock.Setup(x => x.GetWorldAsync(worldId))
            .ReturnsAsync((WorldRegistry?)null);

        // Act
        var result = await _controller.GetWorld(worldId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }
}