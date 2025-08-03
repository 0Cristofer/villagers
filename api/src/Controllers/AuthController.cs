using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Villagers.Api.Entities;
using Villagers.Api.Extensions;
using Villagers.Api.Models;
using Villagers.Api.Services;

namespace Villagers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<PlayerEntity> _userManager;
    private readonly SignInManager<PlayerEntity> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<PlayerEntity> userManager,
        SignInManager<PlayerEntity> signInManager,
        IJwtService jwtService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Create domain object first
        var domainPlayer = new Domain.Player(Guid.NewGuid(), request.Username);
        
        // Convert to entity for persistence
        var playerEntity = domainPlayer.ToEntity();

        var result = await _userManager.CreateAsync(playerEntity, request.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        _logger.LogInformation("User {Username} registered successfully", request.Username);

        var token = _jwtService.GenerateToken(domainPlayer);
        var expiresAt = _jwtService.GetTokenExpiration();

        return Ok(new AuthResponse
        {
            Token = token,
            Player = domainPlayer.ToModel(),
            ExpiresAt = expiresAt
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var playerEntity = await _userManager.FindByNameAsync(request.Username);
        if (playerEntity == null)
        {
            return Unauthorized("Invalid username or password");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(playerEntity, request.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized("Invalid username or password");
        }

        _logger.LogInformation("User {Username} logged in successfully", request.Username);

        // Convert entity to domain for business logic
        var domainPlayer = playerEntity.ToDomain();

        var token = _jwtService.GenerateToken(domainPlayer);
        var expiresAt = _jwtService.GetTokenExpiration();

        return Ok(new AuthResponse
        {
            Token = token,
            Player = domainPlayer.ToModel(),
            ExpiresAt = expiresAt
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

        var playerEntity = await _userManager.FindByIdAsync(playerId.ToString());
        if (playerEntity == null)
        {
            return Unauthorized("User no longer exists");
        }

        var domainPlayer = playerEntity.ToDomain();
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

        var playerEntity = await _userManager.FindByIdAsync(playerId.ToString());
        if (playerEntity == null)
        {
            return Unauthorized("User no longer exists");
        }

        _logger.LogInformation("Token refreshed for user {Username}", playerEntity.UserName);

        var domainPlayer = playerEntity.ToDomain();
        var newToken = _jwtService.GenerateToken(domainPlayer);
        var expiresAt = _jwtService.GetTokenExpiration();

        return Ok(new AuthResponse
        {
            Token = newToken,
            Player = domainPlayer.ToModel(),
            ExpiresAt = expiresAt
        });
    }
}