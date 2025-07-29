using Microsoft.AspNetCore.Mvc;
using Villagers.Api.Infrastructure.Repositories;
using Villagers.Shared.Entities;

namespace Villagers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<PlayerController> _logger;

    public PlayerController(IPlayerRepository playerRepository, ILogger<PlayerController> logger)
    {
        _playerRepository = playerRepository;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlayer(string id)
    {
        _logger.LogInformation("Getting player {PlayerId}", id);
        
        var player = await _playerRepository.GetByIdAsync(id);
        if (player == null)
            return NotFound(new { error = "Player not found" });

        return Ok(player);
    }

    [HttpGet("username/{username}")]
    public async Task<IActionResult> GetPlayerByUsername(string username)
    {
        _logger.LogInformation("Getting player by username {Username}", username);
        
        var player = await _playerRepository.GetByUsernameAsync(username);
        if (player == null)
            return NotFound(new { error = "Player not found" });

        return Ok(player);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerRequest request)
    {
        _logger.LogInformation("Creating player with username {Username}", request.Username);

        // Check if username already exists
        if (await _playerRepository.UsernameExistsAsync(request.Username))
            return Conflict(new { error = "Username already exists" });

        // Check if email already exists
        if (await _playerRepository.EmailExistsAsync(request.Email))
            return Conflict(new { error = "Email already exists" });

        var player = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username,
            Email = request.Email
        };

        try
        {
            var createdPlayer = await _playerRepository.CreateAsync(player);
            _logger.LogInformation("Player {PlayerId} created successfully", createdPlayer.Id);
            
            return CreatedAtAction(nameof(GetPlayer), new { id = createdPlayer.Id }, createdPlayer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create player");
            return StatusCode(500, new { error = "Failed to create player" });
        }
    }

    [HttpHead("username/{username}")]
    public async Task<IActionResult> CheckUsernameExists(string username)
    {
        var exists = await _playerRepository.UsernameExistsAsync(username);
        return exists ? Ok() : NotFound();
    }

    [HttpHead("email/{email}")]
    public async Task<IActionResult> CheckEmailExists(string email)
    {
        var exists = await _playerRepository.EmailExistsAsync(email);
        return exists ? Ok() : NotFound();
    }
}

public class CreatePlayerRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}