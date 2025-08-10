using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Domain.Commands.Requests;

namespace Villagers.GameServer.Services;

public interface IGameSimulationService : IHostedService
{
    Task<ICommand> ProcessCommandRequest(ICommandRequest request);
    Guid GetWorldId();
    bool IsPlayerRegistered(Guid playerId);
}