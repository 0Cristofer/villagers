using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Villagers.GameServer.Configuration;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Domain.Commands.Requests;
using Villagers.GameServer.Domain.Enums;
using Villagers.GameServer.Extensions;
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
    private readonly Mock<IGamePersistenceService> _gamePersistenceServiceMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IWorldPersistenceBackgroundService> _worldPersistenceServiceMock;
    private readonly GameSimulationService _service;

    public GameSimulationServiceTests()
    {
        _loggerMock = new Mock<ILogger<GameSimulationService>>();
        _hubContextMock = new Mock<IHubContext<GameHub, IGameClient>>();
        _worldConfigMock = new Mock<IOptions<WorldConfiguration>>();
        _worldRegistrationServiceMock = new Mock<IWorldRegistrationService>();
        _gamePersistenceServiceMock = new Mock<IGamePersistenceService>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _worldPersistenceServiceMock = new Mock<IWorldPersistenceBackgroundService>();
        
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
        
        // Setup persistence service to return no persisted world by default
        _gamePersistenceServiceMock.Setup(x => x.GetWorldAsync()).ReturnsAsync((World?)null);
        _gamePersistenceServiceMock.Setup(x => x.GetPersistedCommandsAsync()).ReturnsAsync(new List<List<ICommand>>());
        
        // Setup service scope factory chain
        _serviceProviderMock.Setup(x => x.GetService(typeof(IGamePersistenceService)))
            .Returns(_gamePersistenceServiceMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        
        _service = new GameSimulationService(
            _loggerMock.Object, 
            _hubContextMock.Object, 
            _worldConfigMock.Object,
            _worldRegistrationServiceMock.Object,
            _serviceScopeFactoryMock.Object,
            _worldPersistenceServiceMock.Object);
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
    public async Task ProcessCommandRequest_ShouldAcceptRequest()
    {
        // Arrange
        await _service.StartAsync(CancellationToken.None);
        var request = new TestCommandRequest(Guid.NewGuid(), "test message");

        // Act
        var exception = await Record.ExceptionAsync(async () => await _service.ProcessCommandRequest(request));

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
    public async Task ProcessCommandRequest_WithMultipleRequests_ShouldNotThrow()
    {
        // Arrange
        await _service.StartAsync(CancellationToken.None);
        var requests = new[]
        {
            new TestCommandRequest(Guid.NewGuid(), "message1"),
            new TestCommandRequest(Guid.NewGuid(), "message2"),
            new TestCommandRequest(Guid.NewGuid(), "message3")
        };

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            foreach (var request in requests)
            {
                await _service.ProcessCommandRequest(request);
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
            registrationServiceMock.Object,
            _serviceScopeFactoryMock.Object,
            _worldPersistenceServiceMock.Object);

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

    [Fact]
    public async Task GetWorldId_ShouldReturnWorldId()
    {
        // Arrange
        await _service.StartAsync(CancellationToken.None);
        
        // Act
        var worldId = _service.GetWorldId();

        // Assert
        worldId.Should().NotBe(Guid.Empty);
    }


    [Fact]
    public async Task ProcessCommandRequest_WithRegisterPlayerRequest_ShouldAcceptRequest()
    {
        // Arrange
        await _service.StartAsync(CancellationToken.None);
        var request = new RegisterPlayerCommandRequest(Guid.NewGuid(), StartingDirection.Random);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _service.ProcessCommandRequest(request));

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public async Task StartAsync_WithPersistedWorld_ShouldRestoreWorldAndReplayCommands()
    {
        // Arrange
        var persistedWorldId = Guid.NewGuid();
        var persistedTickNumber = 42;
        var config = new WorldConfiguration
        {
            WorldName = "Persisted World",
            TickInterval = TimeSpan.FromMilliseconds(100)
        };
        
        // Create a persisted world
        var persistedWorld = new World(persistedWorldId, config.ToDomain(), new CommandQueue(), persistedTickNumber);
        
        // Create some persisted commands grouped by tick
        var persistedCommands = new List<List<ICommand>>
        {
            // Tick 43 commands
            new List<ICommand>
            {
                new TestCommand(Guid.NewGuid(), "Command from tick 43", 43)
            },
            // Tick 44 commands  
            new List<ICommand>
            {
                new TestCommand(Guid.NewGuid(), "First command from tick 44", 44),
                new TestCommand(Guid.NewGuid(), "Second command from tick 44", 44)
            }
        };
        
        // Setup mocks to return persisted data
        _gamePersistenceServiceMock.Setup(x => x.GetWorldAsync())
            .ReturnsAsync(persistedWorld);
        _gamePersistenceServiceMock.Setup(x => x.GetPersistedCommandsAsync())
            .ReturnsAsync(persistedCommands);
        
        var service = new GameSimulationService(
            _loggerMock.Object,
            _hubContextMock.Object,
            _worldConfigMock.Object,
            _worldRegistrationServiceMock.Object,
            _serviceScopeFactoryMock.Object,
            _worldPersistenceServiceMock.Object);

        // Act - Start the service but stop it immediately to prevent ongoing execution
        await service.StartAsync(CancellationToken.None);
        
        // Stop the service immediately to prevent the background execution from continuing
        await service.StopAsync(CancellationToken.None);

        // Assert
        // Verify that GetWorldAsync was called to check for persisted world
        _gamePersistenceServiceMock.Verify(x => x.GetWorldAsync(), Times.Once);
        
        // Verify that GetPersistedCommandsAsync was called to get commands to replay
        _gamePersistenceServiceMock.Verify(x => x.GetPersistedCommandsAsync(), Times.Once);
        
        // Verify the world was restored with correct properties
        service.GetWorldId().Should().Be(persistedWorldId);
        
        // Verify that commands were replayed and world tick advanced
        // World starts at tick 42, after processing 2 command groups it ends up at tick 44,
        // but ExecuteAsync runs one more tick making it 45
        service.GetCurrentTickNumber().Should().Be(45);
        
        // Verify logging of restoration
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Found persisted world at tick 42")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
            
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Replaying 2 tick groups")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WithPersistedWorld_ShouldUsePersistedConfigDuringReplayThenSwitchToCurrent()
    {
        // Arrange
        var persistedWorldId = Guid.NewGuid();
        var persistedTickNumber = 10;
        
        // Persisted world has different config
        var persistedConfig = new WorldConfig("Persisted World", TimeSpan.FromMilliseconds(500));
        var persistedWorld = new World(persistedWorldId, persistedConfig, new CommandQueue(), persistedTickNumber);
        
        // Current config is different
        var currentConfig = new WorldConfiguration
        {
            WorldName = "Current World",
            TickInterval = TimeSpan.FromMilliseconds(2000)
        };
        
        var persistedCommands = new List<List<ICommand>>
        {
            new List<ICommand>
            {
                new TestCommand(Guid.NewGuid(), "Replayed command", 11)
            }
        };
        
        _gamePersistenceServiceMock.Setup(x => x.GetWorldAsync())
            .ReturnsAsync(persistedWorld);
        _gamePersistenceServiceMock.Setup(x => x.GetPersistedCommandsAsync())
            .ReturnsAsync(persistedCommands);
        
        var service = new GameSimulationService(
            _loggerMock.Object,
            _hubContextMock.Object,
            Options.Create(currentConfig),
            _worldRegistrationServiceMock.Object,
            _serviceScopeFactoryMock.Object,
            _worldPersistenceServiceMock.Object);

        // Act
        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        // Assert
        service.GetWorldId().Should().Be(persistedWorldId);
        service.GetCurrentTickNumber().Should().Be(12); // 10 + 1 (replay) + 1 (ExecuteAsync)
        
        // Verify configuration was updated after replay
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Updated world configuration to current settings after command replay")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact] 
    public async Task StartAsync_WithConfigMismatch_ShouldEnsureSimulationConsistency()
    {
        // Arrange - Test that commands are replayed with original config for consistency
        var persistedConfig = new WorldConfig("Original", TimeSpan.FromMilliseconds(100));
        var persistedWorld = new World(Guid.NewGuid(), persistedConfig, new CommandQueue(), 5);
        
        // Current config has different timing that could affect simulation
        var currentConfig = new WorldConfiguration
        {
            WorldName = "Updated", 
            TickInterval = TimeSpan.FromSeconds(5) // Much different timing
        };
        
        var persistedCommands = new List<List<ICommand>>
        {
            new List<ICommand> { new TestCommand(Guid.NewGuid(), "cmd1", 6) },
            new List<ICommand> { new TestCommand(Guid.NewGuid(), "cmd2", 7) }
        };
        
        _gamePersistenceServiceMock.Setup(x => x.GetWorldAsync())
            .ReturnsAsync(persistedWorld);
        _gamePersistenceServiceMock.Setup(x => x.GetPersistedCommandsAsync())
            .ReturnsAsync(persistedCommands);
        
        var service = new GameSimulationService(
            _loggerMock.Object,
            _hubContextMock.Object,
            Options.Create(currentConfig),
            _worldRegistrationServiceMock.Object,
            _serviceScopeFactoryMock.Object,
            _worldPersistenceServiceMock.Object);

        // Act
        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        // Assert - Verify both replay and config update occurred
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Replaying 2 tick groups")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
            
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Command replay complete. World is now at tick 7")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
            
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Updated world configuration to current settings after command replay")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

}