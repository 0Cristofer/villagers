using Microsoft.AspNetCore.Mvc;

namespace Villagers.GameServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommandController : ControllerBase
{
    private readonly ILogger<CommandController> _logger;

    public CommandController(ILogger<CommandController> logger)
    {
        _logger = logger;
    }

    [HttpPost("execute")]
    public IActionResult ExecuteCommand([FromBody] object command)
    {
        _logger.LogInformation("Received command: {Command}", command);
        
        // TODO: Add command to queue for processing on next game tick
        // TODO: Validate command
        
        return Ok(new { status = "Command queued for processing" });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}