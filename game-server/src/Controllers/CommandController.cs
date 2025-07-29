using Microsoft.AspNetCore.Mvc;
using Villagers.GameServer.Controllers.Requests;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Services;

namespace Villagers.GameServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommandController : ControllerBase
{
    private readonly ILogger<CommandController> _logger;
    private readonly IGameSimulationService _gameService;

    public CommandController(ILogger<CommandController> logger, IGameSimulationService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    [HttpPost("test")]
    public IActionResult TestCommand([FromBody] TestCommandRequest request)
    {
        _logger.LogInformation("Received test command from player {PlayerId}: {Message}", request.PlayerId, request.Message);
        
        var command = new TestCommand(request.PlayerId, request.Message);
        _gameService.EnqueueCommand(command);
        
        return Ok(new { status = "Test command queued for processing" });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}