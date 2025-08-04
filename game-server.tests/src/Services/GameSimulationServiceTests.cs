using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Villagers.GameServer.Configuration;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Interfaces;
using Villagers.GameServer.Services;
using Xunit;

namespace Villagers.GameServer.Tests.Services;

public class GameSimulationServiceTests
{
    private readonly Mock<ILogger<GameSimulationService>> _loggerMock;
    private readonly Mock<IHubContext<GameHub, IGameClient>> _hubContextMock;
    private readonly Mock<IOptions<WorldConfiguration>> _worldConfigMock;
    private readonly Mock<IWorldRegistrationService> _worldRegistrationServiceMock;
    private readonly GameSimulationService _service;

    public GameSimulationServiceTests()
    {
        _loggerMock = new Mock<ILogger<GameSimulationService>>();
        _hubContextMock = new Mock<IHubContext<GameHub, IGameClient>>();
        _worldConfigMock = new Mock<IOptions<WorldConfiguration>>();
        _worldRegistrationServiceMock = new Mock<IWorldRegistrationService>();
        
        var clientsMock = new Mock<IHubClients<IGameClient>>();
        var clientProxyMock = new Mock<IGameClient>();
        
        _hubContextMock.Setup(x => x.Clients).Returns(clientsMock.Object);
        clientsMock.Setup(x => x.All).Returns(clientProxyMock.Object);
        
        // Setup world configuration
        var worldConfig = new WorldConfiguration
        {
            WorldName = "Test World",
            TickInterval = TimeSpan.FromMilliseconds(100)
        };
        _worldConfigMock.Setup(x => x.Value).Returns(worldConfig);
        
        
        _service = new GameSimulationService(
            _loggerMock.Object, 
            _hubContextMock.Object, 
            _worldConfigMock.Object,
            _worldRegistrationServiceMock.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeWorldAndCommandQueue()
    {
        // Assert
        _service.Should().NotBeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Game Simulation Service started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never); // Not started yet
    }

    [Fact]
    public void EnqueueCommand_ShouldAcceptCommand()
    {
        // Arrange
        var command = new TestCommand(Guid.NewGuid(), "test message");

        // Act
        var exception = Record.Exception(() => _service.EnqueueCommand(command));

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogStartAndStop()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100)); // Cancel quickly for testing

        // Act
        try
        {
            await _service.StartAsync(cts.Token);
            await Task.Delay(50); // Let it run briefly
            await _service.StopAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Game Simulation Service started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void EnqueueCommand_WithMultipleCommands_ShouldNotThrow()
    {
        // Arrange
        var commands = new[]
        {
            new TestCommand(Guid.NewGuid(), "message1"),
            new TestCommand(Guid.NewGuid(), "message2"),
            new TestCommand(Guid.NewGuid(), "message3")
        };

        // Act
        var exception = Record.Exception(() =>
        {
            foreach (var command in commands)
            {
                _service.EnqueueCommand(command);
            }
        });

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithRegistrationFailure_ShouldThrowException()
    {
        // Arrange
        var registrationServiceMock = new Mock<IWorldRegistrationService>();
        registrationServiceMock.Setup(x => x.RegisterWorldAsync(It.IsAny<Villagers.GameServer.Domain.World>()))
            .ThrowsAsync(new InvalidOperationException("Registration failed"));

        var service = new GameSimulationService(
            _loggerMock.Object,
            _hubContextMock.Object,
            _worldConfigMock.Object,
            registrationServiceMock.Object);

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.StartAsync(CancellationToken.None);
        });

        exception.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Contain("Registration failed");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRegistrationService()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        try
        {
            await _service.StartAsync(cts.Token);
            await Task.Delay(50, cts.Token);
            await _service.StopAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }

        // Assert
        _worldRegistrationServiceMock.Verify(x => x.RegisterWorldAsync(It.IsAny<Villagers.GameServer.Domain.World>()), Times.Once);
        _worldRegistrationServiceMock.Verify(x => x.UnregisterWorldAsync(It.IsAny<Villagers.GameServer.Domain.World>()), Times.Once);
    }



}