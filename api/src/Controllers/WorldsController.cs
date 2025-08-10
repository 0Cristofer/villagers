using Microsoft.AspNetCore.Mvc;
using Villagers.Api.Extensions;
using Villagers.Api.Models;
using Villagers.Api.Services;

namespace Villagers.Api.Controllers;

[ApiController]
[Route("api/worlds")]
public class WorldsController : ControllerBase
{
    private readonly IWorldRegistryService _worldRegistryService;

    public WorldsController(IWorldRegistryService worldRegistryService)
    {
        _worldRegistryService = worldRegistryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorldResponse>>> GetWorlds()
    {
        var worlds = await _worldRegistryService.GetAllWorldsAsync();
        var worldResponses = worlds.Select(w => w.ToModel());
        return Ok(worldResponses);
    }

    [HttpGet("{worldId:guid}")]
    public async Task<ActionResult<WorldResponse>> GetWorld(Guid worldId)
    {
        var world = await _worldRegistryService.GetWorldAsync(worldId);
        if (world == null)
        {
            return NotFound();
        }

        return Ok(world.ToModel());
    }
}