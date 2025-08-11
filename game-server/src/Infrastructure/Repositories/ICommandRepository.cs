using Villagers.GameServer.Domain.Commands.Requests;

namespace Villagers.GameServer.Infrastructure.Repositories;

public interface ICommandRepository
{
    Task<List<List<ReplayableCommandRequest>>> GetReplayableCommandRequestsGroupedByTickAsync();
    Task SaveCommandRequestAsync(ICommandRequest commandRequest);
    Task DeleteCommandRequestsBeforeTickAsync(long tickNumber);
}