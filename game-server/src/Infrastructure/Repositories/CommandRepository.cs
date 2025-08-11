using Microsoft.EntityFrameworkCore;
using Villagers.GameServer.Domain.Commands.Requests;
using Villagers.GameServer.Infrastructure.Data;
using Villagers.GameServer.Infrastructure.Extensions;

namespace Villagers.GameServer.Infrastructure.Repositories;

public class CommandRepository : ICommandRepository
{
    private readonly GameDbContext _context;

    public CommandRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task<List<List<ReplayableCommandRequest>>> GetReplayableCommandRequestsGroupedByTickAsync()
    {
        // Group first, then order groups by tick number, then order within groups by timestamp
        var groupedEntities = await _context.CommandRequests
            .GroupBy(c => c.TickNumber)
            .OrderBy(g => g.Key) // Order groups by tick number
            .Select(g => new { 
                TickNumber = g.Key, 
                CommandRequests = g.OrderBy(c => c.CreatedAt).ToList() // Order within each group by timestamp
            })
            .ToListAsync();

        // Convert to domain objects and maintain the grouped structure
        var result = new List<List<ReplayableCommandRequest>>();
        foreach (var group in groupedEntities)
        {
            var tickCommandRequests = group.CommandRequests
                .Select(e => e.ToReplayableRequest())
                .ToList();
                
            if (tickCommandRequests.Count > 0)
            {
                result.Add(tickCommandRequests);
            }
        }

        return result;
    }

    public async Task SaveCommandRequestAsync(ICommandRequest commandRequest)
    {
        if (!commandRequest.ProcessedTickNumber.HasValue)
            throw new InvalidOperationException("Cannot persist command request without processed tick number");

        var entity = commandRequest.ToEntity();
        _context.CommandRequests.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCommandRequestsBeforeTickAsync(long tickNumber)
    {
        // Delete all command requests with tick number less than the specified tick
        var commandRequestsToDelete = _context.CommandRequests
            .Where(c => c.TickNumber < tickNumber);
            
        _context.CommandRequests.RemoveRange(commandRequestsToDelete);
        await _context.SaveChangesAsync();
    }
}