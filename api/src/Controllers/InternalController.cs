using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Villagers.Api.Data;
using Villagers.Api.Domain;
using Villagers.Api.Extensions;
using Villagers.Api.Models;

namespace Villagers.Api.Controllers;

[ApiController]
[Route("api/internal")]
public class InternalController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly IConfiguration _configuration;

    public InternalController(ApiDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("worlds/register")]
    public async Task<IActionResult> RegisterWorld([FromBody] RegisterWorldRequest request)
    {
        // API Key authentication
        if (!IsValidApiKey())
        {
            return Unauthorized("Invalid API key");
        }

        // Create domain object
        var worldRegistry = request.ToDomain();

        // Convert to entity and persist
        var entity = worldRegistry.ToEntity();
        _context.WorldRegistry.Add(entity);
        await _context.SaveChangesAsync();

        return Ok(new { Id = worldRegistry.Id });
    }

    [HttpDelete("worlds/{id}")]
    public async Task<IActionResult> UnregisterWorld(Guid id)
    {
        // API Key authentication
        if (!IsValidApiKey())
        {
            return Unauthorized("Invalid API key");
        }

        var entity = await _context.WorldRegistry.FirstOrDefaultAsync(w => w.WorldId == id);
        if (entity == null)
        {
            return NotFound();
        }

        _context.WorldRegistry.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
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