using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Villagers.Api.Controllers;
using Villagers.Api.Data;
using Villagers.Api.Models;
using Xunit;

namespace Villagers.Api.Tests.Controllers;

public class InternalControllerTests : IDisposable
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ApiDbContext _context;
    private readonly InternalController _controller;

    public InternalControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApiDbContext(options);
        _configurationMock = new Mock<IConfiguration>();
        
        // Setup valid API key for default tests
        _configurationMock.Setup(x => x["InternalApi:ApiKey"]).Returns("test-api-key");
        
        _controller = new InternalController(_context, _configurationMock.Object);

        // Setup HTTP context with API key header
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-API-Key"] = "test-api-key";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task RegisterWorld_WithValidRequest_ShouldCreateWorldRegistry()
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
        var result = await _controller.RegisterWorld(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().NotBeNull();

        var worldRegistry = await _context.WorldRegistry.FirstOrDefaultAsync();
        worldRegistry.Should().NotBeNull();
        worldRegistry!.WorldId.Should().Be(request.WorldId);
        worldRegistry.ServerEndpoint.Should().Be(request.ServerEndpoint);
        worldRegistry.Config.WorldName.Should().Be(request.Config.WorldName);
        worldRegistry.Config.TickInterval.Should().Be(request.Config.TickInterval);
    }

    [Fact]
    public async Task RegisterWorld_WithInvalidApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-API-Key"] = "invalid-key";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

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
        var result = await _controller.RegisterWorld(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task RegisterWorld_WithMissingApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

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
        var result = await _controller.RegisterWorld(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UnregisterWorld_WithValidWorldId_ShouldRemoveWorldRegistry()
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

        // First register a world
        await _controller.RegisterWorld(request);
        var registeredWorld = await _context.WorldRegistry.FirstAsync();

        // Act
        var result = await _controller.UnregisterWorld(worldId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        var worldRegistry = await _context.WorldRegistry.FindAsync(registeredWorld.Id);
        worldRegistry.Should().BeNull();
    }

    [Fact]
    public async Task UnregisterWorld_WithNonExistentWorldId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentWorldId = Guid.NewGuid();

        // Act
        var result = await _controller.UnregisterWorld(nonExistentWorldId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UnregisterWorld_WithInvalidApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-API-Key"] = "invalid-key";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var worldId = Guid.NewGuid();

        // Act
        var result = await _controller.UnregisterWorld(worldId);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task RegisterWorld_WithInvalidWorldName_ShouldFailValidation(string? worldName)
    {
        // Arrange
        var request = new RegisterWorldRequest
        {
            WorldId = Guid.NewGuid(),
            ServerEndpoint = "https://localhost:5034/gamehub",
            Config = new WorldConfigModel
            {
                WorldName = worldName!,
                TickInterval = TimeSpan.FromSeconds(5)
            }
        };

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
        {
            await _controller.RegisterWorld(request);
        });

        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public async Task RegisterWorld_WithZeroTickInterval_ShouldFailValidation()
    {
        // Arrange
        var request = new RegisterWorldRequest
        {
            WorldId = Guid.NewGuid(),
            ServerEndpoint = "https://localhost:5034/gamehub",
            Config = new WorldConfigModel
            {
                WorldName = "Test World",
                TickInterval = TimeSpan.Zero
            }
        };

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
        {
            await _controller.RegisterWorld(request);
        });

        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public async Task RegisterWorld_WithNegativeTickInterval_ShouldFailValidation()
    {
        // Arrange
        var request = new RegisterWorldRequest
        {
            WorldId = Guid.NewGuid(),
            ServerEndpoint = "https://localhost:5034/gamehub",
            Config = new WorldConfigModel
            {
                WorldName = "Test World",
                TickInterval = TimeSpan.FromSeconds(-1)
            }
        };

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
        {
            await _controller.RegisterWorld(request);
        });

        exception.Should().BeOfType<ArgumentException>();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}