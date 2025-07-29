using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Villagers.GameServer.Controllers;
using Villagers.GameServer.Controllers.Requests;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Services;
using Xunit;

namespace Villagers.GameServer.Tests.Controllers;

public class CommandControllerTests
{
    private readonly Mock<ILogger<CommandController>> _loggerMock;
    private readonly Mock<IGameSimulationService> _gameServiceMock;
    private readonly CommandController _controller;

    public CommandControllerTests()
    {
        _loggerMock = new Mock<ILogger<CommandController>>();
        _gameServiceMock = new Mock<IGameSimulationService>();
        _controller = new CommandController(_loggerMock.Object, _gameServiceMock.Object);
    }

    [Fact]
    public void TestCommand_ShouldEnqueueCommand_AndReturnOk()
    {
        // Arrange
        var request = new TestCommandRequest
        {
            PlayerId = "player123",
            Message = "Test message"
        };

        // Act
        var result = _controller.TestCommand(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(new { status = "Test command queued for processing" });

        _gameServiceMock.Verify(x => x.EnqueueCommand(
            It.Is<TestCommand>(cmd => cmd.PlayerId == request.PlayerId && cmd.Message == request.Message)),
            Times.Once);
    }

    [Fact]
    public void TestCommand_ShouldLogInformation()
    {
        // Arrange
        var request = new TestCommandRequest
        {
            PlayerId = "player456",
            Message = "Another test message"
        };

        // Act
        _controller.TestCommand(request);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Received test command from player")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Health_ShouldReturnOkWithHealthyStatus()
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