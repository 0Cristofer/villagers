using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Villagers.Api.Controllers.Requests;
using Villagers.Api.Infrastructure.Repositories;
using Villagers.Api.Services;
using Villagers.Shared.Entities;

namespace Villagers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommandController : ControllerBase
{
    private readonly ILogger<CommandController> _logger;
    private readonly ICommandService _commandService;
    private readonly ICommandRepository _commandRepository;

    public CommandController(
        ILogger<CommandController> logger, 
        ICommandService commandService,
        ICommandRepository commandRepository)
    {
        _logger = logger;
        _commandService = commandService;
        _commandRepository = commandRepository;
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestCommand([FromBody] TestCommandRequest request)
    {
        _logger.LogInformation("API received test command from player {PlayerId}: {Message}", request.PlayerId, request.Message);
        
        try
        {
            // First, persist the command to the database
            var command = new Command
            {
                Type = "TestCommand",
                Payload = JsonSerializer.Serialize(request),
                PlayerId = request.PlayerId,
                Status = CommandStatus.Pending
            };

            var persistedCommand = await _commandRepository.CreateAsync(command);
            _logger.LogInformation("Command {CommandId} persisted to database", persistedCommand.Id);

            // Then, send to game server
            var success = await _commandService.SendTestCommandAsync(request);
            
            if (success)
            {
                return Ok(new { 
                    status = "Command sent to game server",
                    commandId = persistedCommand.Id
                });
            }
            else
            {
                // Update command status to failed if game server communication fails
                await _commandRepository.UpdateStatusAsync(persistedCommand.Id, CommandStatus.Failed, "Failed to send to game server");
                return StatusCode(500, new { error = "Failed to send command to game server" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process test command");
            return StatusCode(500, new { error = "Failed to process command" });
        }
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