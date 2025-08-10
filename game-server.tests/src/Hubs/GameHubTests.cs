// TODO: Re-implement tests after architecture changes
// using FluentAssertions;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
// using Moq;
// using Villagers.GameServer.Domain.Commands;
// using Villagers.GameServer.Domain.Commands.Requests;
// using Villagers.GameServer.Domain.Enums;
// using Villagers.GameServer.Hubs;
// using Villagers.GameServer.Services;
// using Xunit;
// 
// namespace Villagers.GameServer.Tests.Hubs;
// 
// public class GameHubTests
// {
//     private readonly Mock<ILogger<GameHub>> _loggerMock;
//     private readonly Mock<IGameSimulationService> _gameServiceMock;
//     private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
//     private readonly GameHub _hub;
// 
//     public GameHubTests()
//     {
//         _loggerMock = new Mock<ILogger<GameHub>>();
//         _gameServiceMock = new Mock<IGameSimulationService>();
//         _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
// 
//         _hub = new GameHub(
//             _loggerMock.Object,
//             _gameServiceMock.Object,
//             _serviceScopeFactoryMock.Object);
//     }
// 
//     [Fact]
//     public async Task RegisterForWorld_WithValidPlayerId_ShouldCallServicesAndEnqueueCommand()
//     {
//         // Arrange
//         var playerId = Guid.NewGuid();
//         var worldId = Guid.NewGuid();
//         var startingDirection = StartingDirection.North;
//         
//         _gameServiceMock.Setup(x => x.GetWorldId()).Returns(worldId);
//         _gameServiceMock.Setup(x => x.GetNextTickNumber()).Returns(6);
// 
//         // Act
//         await _hub.RegisterForWorld(playerId, startingDirection);
// 
//         // Assert
//         _playerRegistrationServiceMock.Verify(x => x.RegisterPlayerForWorldAsync(playerId, worldId), Times.Once);
//         _gameServiceMock.Verify(x => x.ProcessCommandRequest(It.Is<RegisterPlayerCommandRequest>(req => 
//             req.PlayerId == playerId && req.StartingDirection == startingDirection)), Times.Once);
//     }
// 
//     [Fact]
//     public async Task RegisterForWorld_WhenApiCallFails_ShouldNotEnqueueCommand()
//     {
//         // Arrange
//         var playerId = Guid.NewGuid();
//         var worldId = Guid.NewGuid();
//         var startingDirection = StartingDirection.South;
//         
//         _gameServiceMock.Setup(x => x.GetWorldId()).Returns(worldId);
//         _playerRegistrationServiceMock.Setup(x => x.RegisterPlayerForWorldAsync(playerId, worldId))
//             .ThrowsAsync(new InvalidOperationException("API call failed"));
// 
//         // Act & Assert
//         var exception = await Record.ExceptionAsync(() => _hub.RegisterForWorld(playerId, startingDirection));
// 
//         exception.Should().BeOfType<InvalidOperationException>();
//         _gameServiceMock.Verify(x => x.ProcessCommandRequest(It.IsAny<ICommandRequest>()), Times.Never);
//     }
// 
//     [Fact]
//     public async Task RegisterForWorld_WithEmptyPlayerId_ShouldThrowArgumentException()
//     {
//         // Arrange
//         var playerId = Guid.Empty;
//         var worldId = Guid.NewGuid();
//         var startingDirection = StartingDirection.East;
//         
//         _gameServiceMock.Setup(x => x.GetWorldId()).Returns(worldId);
// 
//         // Act & Assert
//         var exception = await Record.ExceptionAsync(() => _hub.RegisterForWorld(playerId, startingDirection));
// 
//         exception.Should().BeOfType<ArgumentException>()
//             .Which.Message.Should().Contain("Player ID cannot be empty");
//         
//         _playerRegistrationServiceMock.Verify(x => x.RegisterPlayerForWorldAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
//         _gameServiceMock.Verify(x => x.ProcessCommandRequest(It.IsAny<ICommandRequest>()), Times.Never);
//     }
// 
//     [Theory]
//     [InlineData(StartingDirection.North)]
//     [InlineData(StartingDirection.South)]
//     [InlineData(StartingDirection.East)]
//     [InlineData(StartingDirection.West)]
//     [InlineData(StartingDirection.Northeast)]
//     [InlineData(StartingDirection.Northwest)]
//     [InlineData(StartingDirection.Southeast)]
//     [InlineData(StartingDirection.Southwest)]
//     [InlineData(StartingDirection.Random)]
//     public async Task RegisterForWorld_WithAllValidDirections_ShouldSucceed(StartingDirection direction)
//     {
//         // Arrange
//         var playerId = Guid.NewGuid();
//         var worldId = Guid.NewGuid();
//         
//         _gameServiceMock.Setup(x => x.GetWorldId()).Returns(worldId);
//         _gameServiceMock.Setup(x => x.GetNextTickNumber()).Returns(11);
// 
//         // Act
//         await _hub.RegisterForWorld(playerId, direction);
// 
//         // Assert
//         _playerRegistrationServiceMock.Verify(x => x.RegisterPlayerForWorldAsync(playerId, worldId), Times.Once);
//         _gameServiceMock.Verify(x => x.ProcessCommandRequest(It.Is<RegisterPlayerCommandRequest>(req => 
//             req.PlayerId == playerId && req.StartingDirection == direction)), Times.Once);
//     }
// }