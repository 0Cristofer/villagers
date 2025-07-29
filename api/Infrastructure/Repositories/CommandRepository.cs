using Microsoft.EntityFrameworkCore;
using Villagers.Api.Infrastructure.Data;
using Villagers.Shared.Entities;

namespace Villagers.Api.Infrastructure.Repositories;

public class CommandRepository : ICommandRepository
{
    private readonly ApiDbContext _context;

    public CommandRepository(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<Command> CreateAsync(Command command)
    {
        command.CreatedAt = DateTime.UtcNow;
        command.Status = CommandStatus.Pending;
        
        _context.Commands.Add(command);
        await _context.SaveChangesAsync();
        return command;
    }

    public async Task<Command?> GetByIdAsync(Guid id)
    {
        return await _context.Commands.FindAsync(id);
    }

    public async Task<IEnumerable<Command>> GetPendingCommandsAsync()
    {
        return await _context.Commands
            .Where(c => c.Status == CommandStatus.Pending)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Command> UpdateStatusAsync(Guid id, CommandStatus status, string? errorMessage = null)
    {
        var command = await _context.Commands.FindAsync(id);
        if (command == null)
            throw new ArgumentException($"Command with ID {id} not found.");

        command.Status = status;
        command.ProcessedAt = DateTime.UtcNow;
        command.ErrorMessage = errorMessage;

        await _context.SaveChangesAsync();
        return command;
    }
}