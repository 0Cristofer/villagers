using Microsoft.AspNetCore.Mvc;
using Villagers.Api.Services;

namespace Villagers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameServerService _gameServerService;
    private readonly ILogger<GameController> _logger;

    public GameController(IGameServerService gameServerService, ILogger<GameController> logger)
    {
        _gameServerService = gameServerService;
        _logger = logger;
    }

    [HttpPost("village/{villageId}/build")]
    public async Task<IActionResult> BuildBuilding(int villageId, [FromBody] BuildBuildingRequest request)
    {
        _logger.LogInformation("Building {BuildingType} in village {VillageId}", request.BuildingType, villageId);
        
        var command = new
        {
            Type = "BuildBuilding",
            VillageId = villageId,
            BuildingType = request.BuildingType,
            PlayerId = GetCurrentPlayerId() // TODO: Get from authentication
        };

        var success = await _gameServerService.SendCommandAsync(command);
        if (success)
        {
            return Ok(new { message = "Build command queued successfully" });
        }

        return StatusCode(500, new { message = "Failed to process command" });
    }

    [HttpPost("village/{villageId}/recruit")]
    public async Task<IActionResult> RecruitTroops(int villageId, [FromBody] RecruitTroopsRequest request)
    {
        _logger.LogInformation("Recruiting {Quantity} {TroopType} in village {VillageId}", 
            request.Quantity, request.TroopType, villageId);
        
        var command = new
        {
            Type = "RecruitTroops",
            VillageId = villageId,
            TroopType = request.TroopType,
            Quantity = request.Quantity,
            PlayerId = GetCurrentPlayerId()
        };

        var success = await _gameServerService.SendCommandAsync(command);
        if (success)
        {
            return Ok(new { message = "Recruitment command queued successfully" });
        }

        return StatusCode(500, new { message = "Failed to process command" });
    }

    [HttpGet("village/{villageId}")]
    public IActionResult GetVillage(int villageId)
    {
        // TODO: Implement village data retrieval
        return Ok(new 
        { 
            id = villageId, 
            name = $"Village {villageId}",
            resources = new { wood = 1000, clay = 800, iron = 600 }
        });
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        var gameServerHealthy = await _gameServerService.IsHealthyAsync();
        
        return Ok(new 
        { 
            api = "healthy", 
            gameServer = gameServerHealthy ? "healthy" : "unhealthy",
            timestamp = DateTime.UtcNow 
        });
    }

    private string GetCurrentPlayerId()
    {
        // TODO: Implement proper authentication and get player ID from token
        return "player1";
    }
}

public record BuildBuildingRequest(string BuildingType);
public record RecruitTroopsRequest(string TroopType, int Quantity);