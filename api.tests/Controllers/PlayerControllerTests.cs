using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Villagers.Api.Controllers;
using Villagers.Api.Infrastructure.Repositories;
using Villagers.Shared.Entities;
using Xunit;

namespace Villagers.Api.Tests.Controllers;

public class PlayerControllerTests
{
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly Mock<ILogger<PlayerController>> _loggerMock;
    private readonly PlayerController _controller;

    public PlayerControllerTests()
    {
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        _loggerMock = new Mock<ILogger<PlayerController>>();
        _controller = new PlayerController(_playerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetPlayer_WhenPlayerExists_ShouldReturnOk()
    {
        // Arrange
        var playerId = "player123";
        var player = new Player
        {
            Id = playerId,
            Username = "testuser",
            Email = "test@example.com"
        };

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        // Act
        var result = await _controller.GetPlayer(playerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(player);
    }

    [Fact]
    public async Task GetPlayer_WhenPlayerDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var playerId = "nonexistent";

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync((Player?)null);

        // Act
        var result = await _controller.GetPlayer(playerId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.Value.Should().BeEquivalentTo(new { error = "Player not found" });
    }

    [Fact]
    public async Task CreatePlayer_WhenValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreatePlayerRequest
        {
            Username = "newuser",
            Email = "newuser@example.com"
        };

        var createdPlayer = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username,
            Email = request.Email
        };

        _playerRepositoryMock
            .Setup(x => x.UsernameExistsAsync(request.Username))
            .ReturnsAsync(false);

        _playerRepositoryMock
            .Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);

        _playerRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Player>()))
            .ReturnsAsync(createdPlayer);

        // Act
        var result = await _controller.CreatePlayer(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(createdPlayer);
        createdResult.ActionName.Should().Be(nameof(PlayerController.GetPlayer));
    }

    [Fact]
    public async Task CreatePlayer_WhenUsernameExists_ShouldReturnConflict()
    {
        // Arrange
        var request = new CreatePlayerRequest
        {
            Username = "existinguser",
            Email = "test@example.com"
        };

        _playerRepositoryMock
            .Setup(x => x.UsernameExistsAsync(request.Username))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreatePlayer(request);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = result as ConflictObjectResult;
        conflictResult!.Value.Should().BeEquivalentTo(new { error = "Username already exists" });
    }

    [Fact]
    public async Task CreatePlayer_WhenEmailExists_ShouldReturnConflict()
    {
        // Arrange
        var request = new CreatePlayerRequest
        {
            Username = "newuser",
            Email = "existing@example.com"
        };

        _playerRepositoryMock
            .Setup(x => x.UsernameExistsAsync(request.Username))
            .ReturnsAsync(false);

        _playerRepositoryMock
            .Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreatePlayer(request);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = result as ConflictObjectResult;
        conflictResult!.Value.Should().BeEquivalentTo(new { error = "Email already exists" });
    }

    [Fact]
    public async Task CheckUsernameExists_WhenUsernameExists_ShouldReturnOk()
    {
        // Arrange
        var username = "existinguser";

        _playerRepositoryMock
            .Setup(x => x.UsernameExistsAsync(username))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CheckUsernameExists(username);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task CheckUsernameExists_WhenUsernameDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var username = "nonexistentuser";

        _playerRepositoryMock
            .Setup(x => x.UsernameExistsAsync(username))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CheckUsernameExists(username);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}