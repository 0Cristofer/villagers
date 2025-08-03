using Villagers.GameServer.DTOs;

namespace Villagers.GameServer.Interfaces;

public interface IGameClient
{
    Task WorldUpdate(WorldStateDto worldState);
    Task CommandReceived(string commandType, string message);
}