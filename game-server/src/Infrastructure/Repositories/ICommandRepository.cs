using Villagers.GameServer.Domain.Commands;

namespace Villagers.GameServer.Infrastructure.Repositories;

public interface ICommandRepository
{
    Task<List<List<ICommand>>> GetCommandsGroupedByTickAsync();
    Task SaveCommandAsync(ICommand command);
    Task DeleteCommandsBeforeTickAsync(long tickNumber);
}