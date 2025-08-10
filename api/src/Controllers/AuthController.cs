using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Villagers.Api.Extensions;
using Villagers.Api.Models;
using Villagers.Api.Services;

namespace Villagers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPlayerAuthenticationService _authService;
    private readonly IPlayerService _playerService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IPlayerAuthenticationService authService,
        IPlayerService playerService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _playerService = playerService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var authenticatedPlayer = await _authService.RegisterAsync(request.Username, request.Password);
            
            _logger.LogInformation("User {Username} registered successfully", request.Username);

            return Ok(new AuthResponse
            {
                Token = authenticatedPlayer.Token.Token,
                Player = authenticatedPlayer.Player.ToModel(),
                ExpiresAt = authenticatedPlayer.Token.ExpiresAt
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return BadRequest(ModelState);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var authenticatedPlayer = await _authService.LoginAsync(request.Username, request.Password);
        if (authenticatedPlayer == null)
        {
            return Unauthorized("Invalid username or password");
        }

        _logger.LogInformation("User {Username} logged in successfully", request.Username);

        return Ok(new AuthResponse
        {
            Token = authenticatedPlayer.Token.Token,
            Player = authenticatedPlayer.Player.ToModel(),
            ExpiresAt = authenticatedPlayer.Token.ExpiresAt
        });
    }

    [HttpPost("validate")]
    [Authorize]
    public async Task<IActionResult> ValidateToken()
    {
        var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (playerIdClaim == null || !Guid.TryParse(playerIdClaim, out var playerId))
        {
            return Unauthorized("Invalid token claims");
        }

        var domainPlayer = await _playerService.GetByIdAsync(playerId);
        if (domainPlayer == null)
        {
            return Unauthorized("User no longer exists");
        }

        return Ok(new { 
            Valid = true, 
            Player = domainPlayer.ToModel() 
        });
    }

    [HttpPost("refresh")]
    [Authorize]
    public async Task<IActionResult> RefreshToken()
    {
        var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (playerIdClaim == null || !Guid.TryParse(playerIdClaim, out var playerId))
        {
            return Unauthorized("Invalid token claims");
        }

        var authenticatedPlayer = await _authService.RefreshTokenAsync(playerId);
        if (authenticatedPlayer == null)
        {
            return Unauthorized("User no longer exists");
        }

        _logger.LogInformation("Token refreshed for user {Username}", authenticatedPlayer.Player.Username);

        return Ok(new AuthResponse
        {
            Token = authenticatedPlayer.Token.Token,
            Player = authenticatedPlayer.Player.ToModel(),
            ExpiresAt = authenticatedPlayer.Token.ExpiresAt
        });
    }
}