using Microsoft.EntityFrameworkCore;
using Villagers.GameServer.Domain.Commands;
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

    public async Task<IEnumerable<ICommand>> GetAllCommandsAsync()
    {
        var entities = await _context.Commands
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
            
        return entities.Select(e => e.ToDomain()).Where(c => c != null).Cast<ICommand>();
    }

    public async Task<IEnumerable<ICommand>> GetCommandsOrderedByTickAndTimestampAsync()
    {
        var entities = await _context.Commands
            .OrderBy(c => c.TickNumber)
            .ThenBy(c => c.CreatedAt)
            .ToListAsync();
            
        return entities.Select(e => e.ToDomain()).Where(c => c != null).Cast<ICommand>();
    }

    public async Task<List<List<ICommand>>> GetCommandsGroupedByTickAsync()
    {
        // Group first, then order groups by tick number, then order within groups by timestamp
        var groupedEntities = await _context.Commands
            .GroupBy(c => c.TickNumber)
            .OrderBy(g => g.Key) // Order groups by tick number
            .Select(g => new { 
                TickNumber = g.Key, 
                Commands = g.OrderBy(c => c.CreatedAt).ToList() // Order within each group by timestamp
            })
            .ToListAsync();

        // Convert to domain objects and maintain the grouped structure
        var result = new List<List<ICommand>>();
        foreach (var group in groupedEntities)
        {
            var tickCommands = group.Commands
                .Select(e => e.ToDomain())
                .Where(c => c != null)
                .Cast<ICommand>()
                .ToList();
                
            if (tickCommands.Count > 0)
            {
                result.Add(tickCommands);
            }
        }

        return result;
    }

    public async Task SaveCommandAsync(ICommand command)
    {
        var entity = command.ToEntity();
        _context.Commands.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCommandAsync(Guid id)
    {
        var command = await _context.Commands.FindAsync(id);
        if (command != null)
        {
            _context.Commands.Remove(command);
            await _context.SaveChangesAsync();
        }
    }
}