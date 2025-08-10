using Microsoft.AspNetCore.Mvc;
using Villagers.Api.Extensions;
using Villagers.Api.Models;
using Villagers.Api.Services;

namespace Villagers.Api.Controllers;

[ApiController]
[Route("api/internal")]
public class InternalController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IPlayerService _playerService;
    private readonly IWorldRegistryService _worldRegistryService;

    public InternalController(IConfiguration configuration, IPlayerService playerService, IWorldRegistryService worldRegistryService)
    {
        _configuration = configuration;
        _playerService = playerService;
        _worldRegistryService = worldRegistryService;
    }

    [HttpPost("worlds/register")]
    public async Task<IActionResult> RegisterWorld([FromBody] RegisterWorldRequest request)
    {
        if (!IsValidApiKey())
        {
            return Unauthorized("Invalid API key");
        }

        await _worldRegistryService.RegisterWorldAsync(request.ToDomain());
        return Ok();
    }

    [HttpDelete("worlds/{id:guid}")]
    public async Task<IActionResult> UnregisterWorld(Guid id)
    {
        if (!IsValidApiKey())
        {
            return Unauthorized("Invalid API key");
        }

        await _worldRegistryService.UnregisterWorldAsync(id);
        return NoContent();
    }

    [HttpPost("players/{playerId:guid}/register-world")]
    public async Task<IActionResult> RegisterPlayerForWorld(Guid playerId, [FromBody] RegisterPlayerForWorldRequest request)
    {
        if (!IsValidApiKey())
        {
            return Unauthorized("Invalid API key");
        }

        try
        {
            await _playerService.RegisterPlayerForWorldAsync(playerId, request.WorldId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    private bool IsValidApiKey()
    {
        var providedKey = Request.Headers["X-API-Key"].FirstOrDefault();
        var expectedKey = _configuration["InternalApi:ApiKey"];
        
        return !string.IsNullOrEmpty(providedKey) && 
               !string.IsNullOrEmpty(expectedKey) && 
               providedKey == expectedKey;
    }
}