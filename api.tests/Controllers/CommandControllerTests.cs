using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Villagers.Api.Controllers;
using Villagers.Api.Controllers.Requests;
using Villagers.Api.Infrastructure.Repositories;
using Villagers.Api.Services;
using Villagers.Shared.Entities;
using Xunit;

namespace Villagers.Api.Tests.Controllers;

public class CommandControllerTests
{
    private readonly Mock<ILogger<CommandController>> _loggerMock;
    private readonly Mock<ICommandService> _commandServiceMock;
    private readonly Mock<ICommandRepository> _commandRepositoryMock;
    private readonly CommandController _controller;

    public CommandControllerTests()
    {
        _loggerMock = new Mock<ILogger<CommandController>>();
        _commandServiceMock = new Mock<ICommandService>();
        _commandRepositoryMock = new Mock<ICommandRepository>();
        _controller = new CommandController(
            _loggerMock.Object, 
            _commandServiceMock.Object,
            _commandRepositoryMock.Object);
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

        var persistedCommand = new Command
        {
            Id = Guid.NewGuid(),
            Type = "TestCommand",
            PlayerId = request.PlayerId,
            Status = CommandStatus.Pending
        };

        _commandRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Command>()))
            .ReturnsAsync(persistedCommand);

        _commandServiceMock
            .Setup(x => x.SendTestCommandAsync(It.IsAny<TestCommandRequest>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.TestCommand(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var responseValue = JsonSerializer.Serialize(okResult!.Value);
        responseValue.Should().Contain("Command sent to game server");
        responseValue.Should().Contain(persistedCommand.Id.ToString());

        _commandRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Command>()), Times.Once);
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

        var persistedCommand = new Command
        {
            Id = Guid.NewGuid(),
            Type = "TestCommand",
            PlayerId = request.PlayerId,
            Status = CommandStatus.Pending
        };

        _commandRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Command>()))
            .ReturnsAsync(persistedCommand);

        _commandRepositoryMock
            .Setup(x => x.UpdateStatusAsync(It.IsAny<Guid>(), CommandStatus.Failed, It.IsAny<string>()))
            .ReturnsAsync(persistedCommand);

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
        
        _commandRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Command>()), Times.Once);
        _commandRepositoryMock.Verify(x => x.UpdateStatusAsync(persistedCommand.Id, CommandStatus.Failed, "Failed to send to game server"), Times.Once);
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

        var persistedCommand = new Command
        {
            Id = Guid.NewGuid(),
            Type = "TestCommand",
            PlayerId = request.PlayerId,
            Status = CommandStatus.Pending
        };

        _commandRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Command>()))
            .ReturnsAsync(persistedCommand);

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
            
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Command") && o.ToString()!.Contains("persisted to database")),
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