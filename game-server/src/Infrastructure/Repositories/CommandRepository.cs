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