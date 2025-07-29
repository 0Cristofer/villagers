using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Villagers.Api.Controllers;
using Villagers.Api.Controllers.Requests;
using Villagers.Api.Services;
using Xunit;

namespace Villagers.Api.Tests.Controllers;

public class CommandControllerTests
{
    private readonly Mock<ILogger<CommandController>> _loggerMock;
    private readonly Mock<ICommandService> _commandServiceMock;
    private readonly CommandController _controller;

    public CommandControllerTests()
    {
        _loggerMock = new Mock<ILogger<CommandController>>();
        _commandServiceMock = new Mock<ICommandService>();
        _controller = new CommandController(_loggerMock.Object, _commandServiceMock.Object);
    }

    [Fact]
    public async Task TestCommand_WhenServiceSucceeds_ShouldReturnOk()
    {
        // Arrange
        var request = new TestCommandRequest
        {
            PlayerId = "player123",
            Message = "Test message"
        };

        _commandServiceMock
            .Setup(x => x.SendTestCommandAsync(It.IsAny<TestCommandRequest>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.TestCommand(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(new { status = "Command sent to game server" });

        _commandServiceMock.Verify(x => x.SendTestCommandAsync(request), Times.Once);
    }

    [Fact]
    public async Task TestCommand_WhenServiceFails_ShouldReturn500()
    {
        // Arrange
        var request = new TestCommandRequest
        {
            PlayerId = "player456",
            Message = "Another test message"
        };

        _commandServiceMock
            .Setup(x => x.SendTestCommandAsync(It.IsAny<TestCommandRequest>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.TestCommand(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().BeEquivalentTo(new { error = "Failed to send command to game server" });
    }

    [Fact]
    public async Task TestCommand_ShouldLogInformation()
    {
        // Arrange
        var request = new TestCommandRequest
        {
            PlayerId = "player789",
            Message = "Log test message"
        };

        _commandServiceMock
            .Setup(x => x.SendTestCommandAsync(It.IsAny<TestCommandRequest>()))
            .ReturnsAsync(true);

        // Act
        await _controller.TestCommand(request);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("API received test command from player")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Health_ShouldReturnOkWithApiHealthStatus()
    {
        // Act
        var result = _controller.Health();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        
        okResult!.Value.Should().NotBeNull();
        var response = JsonSerializer.Serialize(okResult.Value);
        response.Should().Contain("healthy");
        response.Should().Contain("timestamp");
    }
}