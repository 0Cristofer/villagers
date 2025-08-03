using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Villagers.Api.Controllers;
using Villagers.Api.Domain;
using Villagers.Api.Entities;
using Villagers.Api.Models;
using Villagers.Api.Services;
using Xunit;

namespace Villagers.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<PlayerEntity>> _userManagerMock;
    private readonly Mock<SignInManager<PlayerEntity>> _signInManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _signInManagerMock = CreateSignInManagerMock();
        _jwtServiceMock = new Mock<IJwtService>();
        _loggerMock = new Mock<ILogger<AuthController>>();

        _controller = new AuthController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _jwtServiceMock.Object,
            _loggerMock.Object);
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturnOkWithAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest { Username = "testuser", Password = "password123" };
        var expectedToken = "generated-jwt-token";
        var expectedExpiration = DateTime.UtcNow.AddDays(1);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<PlayerEntity>(), request.Password))
                       .ReturnsAsync(IdentityResult.Success);

        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<Player>()))
                      .Returns(expectedToken);

        _jwtServiceMock.Setup(x => x.GetTokenExpiration())
                      .Returns(expectedExpiration);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var authResponse = okResult!.Value as AuthResponse;

        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().Be(expectedToken);
        authResponse.Player.Username.Should().Be(request.Username);
        authResponse.Player.RegisteredWorldIds.Should().BeEmpty();
        authResponse.ExpiresAt.Should().Be(expectedExpiration);
    }

    [Fact]
    public async Task Register_WithInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest { Username = "", Password = "password123" };
        _controller.ModelState.AddModelError("Username", "Username is required");

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<PlayerEntity>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Register_WhenUserCreationFails_ShouldReturnBadRequestWithErrors()
    {
        // Arrange
        var request = new RegisterRequest { Username = "testuser", Password = "weak" };
        var identityErrors = new[]
        {
            new IdentityError { Code = "PasswordTooShort", Description = "Password is too short" },
            new IdentityError { Code = "UsernameTaken", Description = "Username is already taken" }
        };
        var identityResult = IdentityResult.Failed(identityErrors);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<PlayerEntity>(), request.Password))
                       .ReturnsAsync(identityResult);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        var modelState = badRequestResult!.Value as SerializableError;
        
        modelState.Should().NotBeNull();
        _jwtServiceMock.Verify(x => x.GenerateToken(It.IsAny<Player>()), Times.Never);
    }

    [Fact]
    public async Task Register_ShouldCreatePlayerEntityWithCorrectProperties()
    {
        // Arrange
        var request = new RegisterRequest { Username = "testuser", Password = "password123" };
        PlayerEntity? capturedEntity = null;

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<PlayerEntity>(), request.Password))
                       .Callback<PlayerEntity, string>((entity, _) => capturedEntity = entity)
                       .ReturnsAsync(IdentityResult.Success);

        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<Player>())).Returns("token");
        _jwtServiceMock.Setup(x => x.GetTokenExpiration()).Returns(DateTime.UtcNow);

        // Act
        await _controller.Register(request);

        // Assert
        capturedEntity.Should().NotBeNull();
        capturedEntity!.UserName.Should().Be(request.Username);
        capturedEntity.RegisteredWorldIds.Should().BeEmpty();
        capturedEntity.Id.Should().NotBe(Guid.Empty);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithAuthResponse()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "password123" };
        var playerEntity = new PlayerEntity 
        { 
            Id = Guid.NewGuid(), 
            UserName = request.Username,
            RegisteredWorldIds = [1, 2, 3]
        };
        var expectedToken = "generated-jwt-token";
        var expectedExpiration = DateTime.UtcNow.AddDays(1);

        _userManagerMock.Setup(x => x.FindByNameAsync(request.Username))
                       .ReturnsAsync(playerEntity);

        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(playerEntity, request.Password, false))
                         .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<Player>()))
                      .Returns(expectedToken);

        _jwtServiceMock.Setup(x => x.GetTokenExpiration())
                      .Returns(expectedExpiration);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var authResponse = okResult!.Value as AuthResponse;

        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().Be(expectedToken);
        authResponse.Player.Username.Should().Be(request.Username);
        authResponse.Player.Id.Should().Be(playerEntity.Id);
        authResponse.Player.RegisteredWorldIds.Should().BeEquivalentTo([1, 2, 3]);
        authResponse.ExpiresAt.Should().Be(expectedExpiration);
    }

    [Fact]
    public async Task Login_WithInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new LoginRequest { Username = "", Password = "password123" };
        _controller.ModelState.AddModelError("Username", "Username is required");

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _userManagerMock.Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Username = "nonexistent", Password = "password123" };

        _userManagerMock.Setup(x => x.FindByNameAsync(request.Username))
                       .ReturnsAsync((PlayerEntity?)null);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be("Invalid username or password");

        _signInManagerMock.Verify(x => x.CheckPasswordSignInAsync(It.IsAny<PlayerEntity>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "wrongpassword" };
        var playerEntity = new PlayerEntity { Id = Guid.NewGuid(), UserName = request.Username };

        _userManagerMock.Setup(x => x.FindByNameAsync(request.Username))
                       .ReturnsAsync(playerEntity);

        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(playerEntity, request.Password, false))
                         .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be("Invalid username or password");

        _jwtServiceMock.Verify(x => x.GenerateToken(It.IsAny<Player>()), Times.Never);
    }

    [Fact]
    public async Task Login_ShouldConvertEntityToDomainBeforeGeneratingToken()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "password123" };
        var playerEntity = new PlayerEntity 
        { 
            Id = Guid.NewGuid(), 
            UserName = request.Username,
            RegisteredWorldIds = [1, 2],
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };
        Player? capturedDomainPlayer = null;

        _userManagerMock.Setup(x => x.FindByNameAsync(request.Username))
                       .ReturnsAsync(playerEntity);

        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(playerEntity, request.Password, false))
                         .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<Player>()))
                      .Callback<Player>(player => capturedDomainPlayer = player)
                      .Returns("token");

        _jwtServiceMock.Setup(x => x.GetTokenExpiration()).Returns(DateTime.UtcNow);

        // Act
        await _controller.Login(request);

        // Assert
        capturedDomainPlayer.Should().NotBeNull();
        capturedDomainPlayer!.Id.Should().Be(playerEntity.Id);
        capturedDomainPlayer.Username.Should().Be(playerEntity.UserName);
        capturedDomainPlayer.RegisteredWorldIds.Should().BeEquivalentTo([1, 2]);
        capturedDomainPlayer.CreatedAt.Should().Be(playerEntity.CreatedAt);
        capturedDomainPlayer.UpdatedAt.Should().Be(playerEntity.UpdatedAt);
    }

    #endregion

    #region ValidateToken Tests

    [Fact]
    public async Task ValidateToken_WithValidTokenAndExistingUser_ShouldReturnOkWithPlayerData()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var playerEntity = new PlayerEntity 
        { 
            Id = playerId, 
            UserName = "testuser",
            RegisteredWorldIds = [1, 2, 3]
        };

        // Mock User.FindFirst to return the player ID claim
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, playerId.ToString())
            }));

        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(playerId.ToString()))
                       .ReturnsAsync(playerEntity);

        // Act
        var result = await _controller.ValidateToken();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value;
        
        response.Should().NotBeNull();
        var responseJson = System.Text.Json.JsonSerializer.Serialize(response);
        responseJson.Should().Contain("\"Valid\":true");
        responseJson.Should().Contain(playerId.ToString());
        responseJson.Should().Contain("testuser");
    }

    [Fact]
    public async Task ValidateToken_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, playerId.ToString())
            }));

        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(playerId.ToString()))
                       .ReturnsAsync((PlayerEntity?)null);

        // Act
        var result = await _controller.ValidateToken();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be("User no longer exists");
    }

    [Fact]
    public async Task ValidateToken_WithInvalidClaims_ShouldReturnUnauthorized()
    {
        // Arrange
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim("SomeOtherClaim", "value")
            }));

        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        // Act
        var result = await _controller.ValidateToken();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be("Invalid token claims");
        
        _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Helper Methods

    private static Mock<UserManager<PlayerEntity>> CreateUserManagerMock()
    {
        var userStoreMock = new Mock<IUserStore<PlayerEntity>>();
        return new Mock<UserManager<PlayerEntity>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static Mock<SignInManager<PlayerEntity>> CreateSignInManagerMock()
    {
        var userManagerMock = CreateUserManagerMock();
        var contextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<PlayerEntity>>();

        return new Mock<SignInManager<PlayerEntity>>(
            userManagerMock.Object,
            contextAccessorMock.Object,
            claimsFactoryMock.Object,
            null, null, null, null);
    }

    #endregion
}