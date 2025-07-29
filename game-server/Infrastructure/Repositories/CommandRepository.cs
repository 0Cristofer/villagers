using Microsoft.EntityFrameworkCore;
using Villagers.GameServer.Infrastructure.Data;
using Villagers.Shared.Entities;

namespace Villagers.GameServer.Infrastructure.Repositories;

public class CommandRepository : ICommandRepository
{
    private readonly GameDbContext _context;

    public CommandRepository(GameDbContext context)
    {
        _context = context;
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

    public async Task<IEnumerable<Command>> GetCommandsBatchAsync(int batchSize = 100)
    {
        return await _context.Commands
            .Where(c => c.Status == CommandStatus.Pending)
            .OrderBy(c => c.CreatedAt)
            .Take(batchSize)
            .ToListAsync();
    }
}