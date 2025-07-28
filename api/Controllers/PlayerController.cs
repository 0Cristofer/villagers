using Microsoft.AspNetCore.Mvc;
using Villagers.Api.Services;

namespace Villagers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly ILogger<PlayerController> _logger;
    private readonly IGameServerService _gameServerService;

    public PlayerController(ILogger<PlayerController> logger, IGameServerService gameServerService)
    {
        _logger = logger;
        _gameServerService = gameServerService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { error = "Username is required" });
        }

        try
        {
            _logger.LogInformation("Processing login for user: {Username}", request.Username);
            
            // Forward the login request to the game server
            var response = await _gameServerService.LoginPlayerAsync(request.Username);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during player login");
            return StatusCode(500, new { error = "An error occurred during login" });
        }
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
}