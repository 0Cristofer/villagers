using Villagers.GameServer.Domain.Commands;

namespace Villagers.GameServer.Services;

public interface IGameSimulationService : IHostedService
{
    void EnqueueCommand(ICommand command);
    Guid GetWorldId();
    int GetCurrentTickNumber();
    int GetNextTickNumber();
}