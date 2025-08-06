using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Domain.Commands.Requests;

namespace Villagers.GameServer.Services;

public interface IGameSimulationService : IHostedService
{
    Task ProcessCommandRequest(ICommandRequest request);
    Guid GetWorldId();
    int GetCurrentTickNumber();
    int GetNextTickNumber();
}