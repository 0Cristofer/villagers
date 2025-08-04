using Villagers.GameServer.Domain;
using Villagers.GameServer.DTOs;

namespace Villagers.GameServer.Extensions;

public static class WorldExtensions
{
    public static WorldStateDto ToDto(this World world)
    {
        return new WorldStateDto(world.Config.WorldName, world.TickNumber, world.Message, DateTime.UtcNow);
    }
}