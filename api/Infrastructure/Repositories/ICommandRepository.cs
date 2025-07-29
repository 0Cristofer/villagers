using Villagers.Shared.Entities;

namespace Villagers.Api.Infrastructure.Repositories;

public interface ICommandRepository
{
    Task<Command> CreateAsync(Command command);
    Task<Command?> GetByIdAsync(Guid id);
    Task<IEnumerable<Command>> GetPendingCommandsAsync();
    Task<Command> UpdateStatusAsync(Guid id, CommandStatus status, string? errorMessage = null);
}