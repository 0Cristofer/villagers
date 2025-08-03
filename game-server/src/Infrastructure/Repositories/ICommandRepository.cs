using Villagers.GameServer.Domain.Commands;

namespace Villagers.GameServer.Infrastructure.Repositories;

public interface ICommandRepository
{
    Task<IEnumerable<ICommand>> GetAllCommandsAsync();
    Task SaveCommandAsync(ICommand command);
    Task DeleteCommandAsync(Guid id);
}