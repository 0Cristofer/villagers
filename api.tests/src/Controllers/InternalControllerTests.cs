// using FluentAssertions;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using Moq;
// using Villagers.Api.Controllers;
// using Villagers.Api.Data;
// using Villagers.Api.Models;
// using Villagers.Api.Services;
// using Xunit;
//
// namespace Villagers.Api.Tests.Controllers;
//
// public class InternalControllerTests
// {
//     private readonly Mock<IConfiguration> _configurationMock;
//     private readonly Mock<IPlayerService> _playerServiceMock;
//     private readonly Mock<IWorldRegistryService> _worldRegistryServiceMock;
//     private readonly InternalController _controller;
//
//     public InternalControllerTests()
//     {
//         _configurationMock = new Mock<IConfiguration>();
//         _playerServiceMock = new Mock<IPlayerService>();
//         _worldRegistryServiceMock = new Mock<IWorldRegistryService>();
//         
//         // Setup valid API key for default tests
//         _configurationMock.Setup(x => x["InternalApi:ApiKey"]).Returns("test-api-key");
//         
//         _controller = new InternalController(_configurationMock.Object, _playerServiceMock.Object, _worldRegistryServiceMock.Object);
//
//         // Setup HTTP context with API key header
//         var httpContext = new DefaultHttpContext();
//         httpContext.Request.Headers["X-API-Key"] = "test-api-key";
//         _controller.ControllerContext = new ControllerContext
//         {
//             HttpContext = httpContext
//         };
//     }
//
//     [Fact]
//     public async Task RegisterWorld_WithValidRequest_ShouldCreateWorldRegistry()
//     {
//         // Arrange
//         var request = new RegisterWorldRequest
//         {
//             WorldId = Guid.NewGuid(),
//             ServerEndpoint = "https://localhost:5034/gamehub",
//             Config = new WorldConfigModel
//             {
//                 WorldName = "Test World",
//                 TickInterval = TimeSpan.FromSeconds(5)
//             }
//         };
//
//         var expectedId = Guid.NewGuid();
//         _worldRegistryServiceMock.Setup(x => x.RegisterWorldAsync(request))
//             .ReturnsAsync(expectedId);
//
//         // Act
//         var result = await _controller.RegisterWorld(request);
//
//         // Assert
//         result.Should().BeOfType<OkObjectResult>();
//         var okResult = (OkObjectResult)result;
//         okResult.Value.Should().NotBeNull();
//
//         _worldRegistryServiceMock.Verify(x => x.RegisterWorldAsync(request), Times.Once);
//     }
//
//     [Fact]
//     public async Task RegisterWorld_WithInvalidApiKey_ShouldReturnUnauthorized()
//     {
//         // Arrange
//         var httpContext = new DefaultHttpContext();
//         httpContext.Request.Headers["X-API-Key"] = "invalid-key";
//         _controller.ControllerContext = new ControllerContext
//         {
//             HttpContext = httpContext
//         };
//
//         var request = new RegisterWorldRequest
//         {
//             WorldId = Guid.NewGuid(),
//             ServerEndpoint = "https://localhost:5034/gamehub",
//             Config = new WorldConfigModel
//             {
//                 WorldName = "Test World",
//                 TickInterval = TimeSpan.FromSeconds(5)
//             }
//         };
//
//         // Act
//         var result = await _controller.RegisterWorld(request);
//
//         // Assert
//         result.Should().BeOfType<UnauthorizedObjectResult>();
//     }
//
//     [Fact]
//     public async Task RegisterWorld_WithMissingApiKey_ShouldReturnUnauthorized()
//     {
//         // Arrange
//         var httpContext = new DefaultHttpContext();
//         _controller.ControllerContext = new ControllerContext
//         {
//             HttpContext = httpContext
//         };
//
//         var request = new RegisterWorldRequest
//         {
//             WorldId = Guid.NewGuid(),
//             ServerEndpoint = "https://localhost:5034/gamehub",
//             Config = new WorldConfigModel
//             {
//                 WorldName = "Test World",
//                 TickInterval = TimeSpan.FromSeconds(5)
//             }
//         };
//
//         // Act
//         var result = await _controller.RegisterWorld(request);
//
//         // Assert
//         result.Should().BeOfType<UnauthorizedObjectResult>();
//     }
//
//     [Fact]
//     public async Task UnregisterWorld_WithValidWorldId_ShouldRemoveWorldRegistry()
//     {
//         // Arrange
//         var worldId = Guid.NewGuid();
//         var request = new RegisterWorldRequest
//         {
//             WorldId = worldId,
//             ServerEndpoint = "https://localhost:5034/gamehub",
//             Config = new WorldConfigModel
//             {
//                 WorldName = "Test World",
//                 TickInterval = TimeSpan.FromSeconds(5)
//             }
//         };
//
//         _worldRegistryServiceMock.Setup(x => x.UnregisterWorldAsync(worldId))
//             .ReturnsAsync(true);
//
//         // Act
//         var result = await _controller.UnregisterWorld(worldId);
//
//         // Assert
//         result.Should().BeOfType<NoContentResult>();
//         _worldRegistryServiceMock.Verify(x => x.UnregisterWorldAsync(worldId), Times.Once);
//     }
//
//     [Fact]
//     public async Task UnregisterWorld_WithNonExistentWorldId_ShouldReturnNotFound()
//     {
//         // Arrange
//         var nonExistentWorldId = Guid.NewGuid();
//         _worldRegistryServiceMock.Setup(x => x.UnregisterWorldAsync(nonExistentWorldId))
//             .ReturnsAsync(false);
//
//         // Act
//         var result = await _controller.UnregisterWorld(nonExistentWorldId);
//
//         // Assert
//         result.Should().BeOfType<NotFoundResult>();
//         _worldRegistryServiceMock.Verify(x => x.UnregisterWorldAsync(nonExistentWorldId), Times.Once);
//     }
//
//     [Fact]
//     public async Task UnregisterWorld_WithInvalidApiKey_ShouldReturnUnauthorized()
//     {
//         // Arrange
//         var httpContext = new DefaultHttpContext();
//         httpContext.Request.Headers["X-API-Key"] = "invalid-key";
//         _controller.ControllerContext = new ControllerContext
//         {
//             HttpContext = httpContext
//         };
//
//         var worldId = Guid.NewGuid();
//
//         // Act
//         var result = await _controller.UnregisterWorld(worldId);
//
//         // Assert
//         result.Should().BeOfType<UnauthorizedObjectResult>();
//     }
//
//     [Theory]
//     [InlineData("")]
//     [InlineData("   ")]
//     [InlineData(null)]
//     public async Task RegisterWorld_WithInvalidWorldName_ShouldFailValidation(string? worldName)
//     {
//         // Arrange
//         var request = new RegisterWorldRequest
//         {
//             WorldId = Guid.NewGuid(),
//             ServerEndpoint = "https://localhost:5034/gamehub",
//             Config = new WorldConfigModel
//             {
//                 WorldName = worldName!,
//                 TickInterval = TimeSpan.FromSeconds(5)
//             }
//         };
//
//         _worldRegistryServiceMock.Setup(x => x.RegisterWorldAsync(request))
//             .ThrowsAsync(new ArgumentException("Invalid world name"));
//
//         // Act & Assert
//         var exception = await Record.ExceptionAsync(async () =>
//         {
//             await _controller.RegisterWorld(request);
//         });
//
//         exception.Should().BeOfType<ArgumentException>();
//     }
//
//     [Fact]
//     public async Task RegisterWorld_WithZeroTickInterval_ShouldFailValidation()
//     {
//         // Arrange
//         var request = new RegisterWorldRequest
//         {
//             WorldId = Guid.NewGuid(),
//             ServerEndpoint = "https://localhost:5034/gamehub",
//             Config = new WorldConfigModel
//             {
//                 WorldName = "Test World",
//                 TickInterval = TimeSpan.Zero
//             }
//         };
//
//         _worldRegistryServiceMock.Setup(x => x.RegisterWorldAsync(request))
//             .ThrowsAsync(new ArgumentException("Invalid tick interval"));
//
//         // Act & Assert
//         var exception = await Record.ExceptionAsync(async () =>
//         {
//             await _controller.RegisterWorld(request);
//         });
//
//         exception.Should().BeOfType<ArgumentException>();
//     }
//
//     [Fact]
//     public async Task RegisterWorld_WithNegativeTickInterval_ShouldFailValidation()
//     {
//         // Arrange
//         var request = new RegisterWorldRequest
//         {
//             WorldId = Guid.NewGuid(),
//             ServerEndpoint = "https://localhost:5034/gamehub",
//             Config = new WorldConfigModel
//             {
//                 WorldName = "Test World",
//                 TickInterval = TimeSpan.FromSeconds(-1)
//             }
//         };
//
//         _worldRegistryServiceMock.Setup(x => x.RegisterWorldAsync(request))
//             .ThrowsAsync(new ArgumentException("Invalid tick interval"));
//
//         // Act & Assert
//         var exception = await Record.ExceptionAsync(async () =>
//         {
//             await _controller.RegisterWorld(request);
//         });
//
//         exception.Should().BeOfType<ArgumentException>();
//     }
//
//     [Fact]
//     public async Task RegisterPlayerForWorld_WithValidData_ShouldCallPlayerService()
//     {
//         // Arrange
//         var playerId = Guid.NewGuid();
//         var worldId = Guid.NewGuid();
//         var request = new RegisterPlayerForWorldRequest { WorldId = worldId };
//
//         // Act
//         var result = await _controller.RegisterPlayerForWorld(playerId, request);
//
//         // Assert
//         result.Should().BeOfType<OkResult>();
//         _playerServiceMock.Verify(x => x.RegisterPlayerForWorldAsync(playerId, worldId), Times.Once);
//     }
//
//     [Fact]
//     public async Task RegisterPlayerForWorld_WhenPlayerNotFound_ShouldReturnNotFound()
//     {
//         // Arrange
//         var playerId = Guid.NewGuid();
//         var worldId = Guid.NewGuid();
//         var request = new RegisterPlayerForWorldRequest { WorldId = worldId };
//
//         _playerServiceMock.Setup(x => x.RegisterPlayerForWorldAsync(playerId, worldId))
//             .ThrowsAsync(new InvalidOperationException("Player not found"));
//
//         // Act
//         var result = await _controller.RegisterPlayerForWorld(playerId, request);
//
//         // Assert
//         result.Should().BeOfType<NotFoundObjectResult>()
//             .Which.Value.Should().Be("Player not found");
//     }
//
//     [Fact]
//     public async Task RegisterPlayerForWorld_WithInvalidApiKey_ShouldReturnUnauthorized()
//     {
//         // Arrange
//         var httpContext = new DefaultHttpContext();
//         httpContext.Request.Headers["X-API-Key"] = "invalid-key";
//         _controller.ControllerContext = new ControllerContext
//         {
//             HttpContext = httpContext
//         };
//
//         var playerId = Guid.NewGuid();
//         var worldId = Guid.NewGuid();
//         var request = new RegisterPlayerForWorldRequest { WorldId = worldId };
//
//         // Act
//         var result = await _controller.RegisterPlayerForWorld(playerId, request);
//
//         // Assert
//         result.Should().BeOfType<UnauthorizedObjectResult>();
//         _playerServiceMock.Verify(x => x.RegisterPlayerForWorldAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
//     }
//
// }