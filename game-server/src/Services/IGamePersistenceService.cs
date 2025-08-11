using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands.Requests;

namespace Villagers.GameServer.Services;

public interface IGamePersistenceService
{
    Task SaveWorldAndClearCommandsAsync(WorldSnapshot worldSnapshot);
    Task<World?> GetWorldAsync();
    Task<List<List<ReplayableCommandRequest>>> GetReplayableCommandRequestsAsync();
    Task SaveCommandRequestAsync(ICommandRequest commandRequest);
}