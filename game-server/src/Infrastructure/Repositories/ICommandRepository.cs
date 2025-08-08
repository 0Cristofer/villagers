using Villagers.GameServer.Domain.Commands;

namespace Villagers.GameServer.Infrastructure.Repositories;

public interface ICommandRepository
{
    Task<IEnumerable<ICommand>> GetAllCommandsAsync();
    Task<IEnumerable<ICommand>> GetCommandsOrderedByTickAndTimestampAsync();
    Task<List<List<ICommand>>> GetCommandsGroupedByTickAsync();
    Task SaveCommandAsync(ICommand command);
    Task DeleteCommandAsync(Guid id);
    Task DeleteCommandsBeforeTickAsync(int tickNumber);
}