namespace Villagers.GameServer.Interfaces;

public interface IGameClient
{
    Task GameStateUpdate(object gameState);
    Task ResourceUpdate(int villageId, object resources);
    Task CombatUpdate(object combatResult);
    Task NotificationUpdate(string message);
}