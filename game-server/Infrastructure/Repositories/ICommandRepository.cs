using Villagers.Shared.Entities;

namespace Villagers.GameServer.Infrastructure.Repositories;

public interface ICommandRepository
{
    Task<IEnumerable<Command>> GetPendingCommandsAsync();
    Task<Command> UpdateStatusAsync(Guid id, CommandStatus status, string? errorMessage = null);
    Task<IEnumerable<Command>> GetCommandsBatchAsync(int batchSize = 100);
}