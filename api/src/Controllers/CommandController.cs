using Microsoft.AspNetCore.Mvc;
using Villagers.Api.Controllers.Requests;
using Villagers.Api.Services;

namespace Villagers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommandController : ControllerBase
{
    private readonly ILogger<CommandController> _logger;
    private readonly ICommandService _commandService;

    public CommandController(ILogger<CommandController> logger, ICommandService commandService)
    {
        _logger = logger;
        _commandService = commandService;
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestCommand([FromBody] TestCommandRequest request)
    {
        _logger.LogInformation("API received test command from player {PlayerId}: {Message}", request.PlayerId, request.Message);
        
        var success = await _commandService.SendTestCommandAsync(request);
        
        if (success)
        {
            return Ok(new { status = "Command sent to game server" });
        }
        
        return StatusCode(500, new { error = "Failed to send command to game server" });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { 
            api = "healthy", 
            timestamp = DateTime.UtcNow 
        });
    }
}