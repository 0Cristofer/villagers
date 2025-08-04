using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Villagers.Api.Data;
using Villagers.Api.Extensions;
using Villagers.Api.Models;

namespace Villagers.Api.Controllers;

[ApiController]
[Route("api/worlds")]
public class WorldsController : ControllerBase
{
    private readonly ApiDbContext _context;

    public WorldsController(ApiDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorldResponse>>> GetWorlds()
    {
        var entities = await _context.WorldRegistry
            .OrderBy(w => w.RegisteredAt)
            .ToListAsync();

        var worlds = entities
            .Select(e => e.ToDomain().ToModel())
            .ToList();

        return Ok(worlds);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorldResponse>> GetWorld(Guid id)
    {
        var entity = await _context.WorldRegistry.FirstOrDefaultAsync(w => w.WorldId == id || w.Id == id);
        if (entity == null)
        {
            return NotFound();
        }

        var world = entity.ToDomain().ToModel();
        return Ok(world);
    }
}