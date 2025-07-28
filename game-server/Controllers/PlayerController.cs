using Microsoft.AspNetCore.Mvc;

namespace Villagers.GameServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly ILogger<PlayerController> _logger;

    public PlayerController(ILogger<PlayerController> logger)
    {
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { error = "Username is required" });
        }

        _logger.LogInformation("Player login attempt: {Username}", request.Username);
        
        // TODO: Check if player exists in game state
        // TODO: Create new player if doesn't exist
        // TODO: Create initial village for new player
        
        // For now, just return success with player info
        var playerInfo = new
        {
            playerId = Guid.NewGuid().ToString(),
            username = request.Username,
            villages = new[]
            {
                new
                {
                    id = 1,
                    name = $"{request.Username}'s Village",
                    x = Random.Shared.Next(0, 100),
                    y = Random.Shared.Next(0, 100),
                    points = 26
                }
            },
            isNewPlayer = true
        };
        
        return Ok(playerInfo);
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
}