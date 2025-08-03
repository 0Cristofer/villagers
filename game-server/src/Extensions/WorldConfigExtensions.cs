using Villagers.GameServer.Domain;
using Villagers.GameServer.Models;

namespace Villagers.GameServer.Extensions;

public static class WorldConfigExtensions
{
    public static WorldConfigModel ToModel(this WorldConfig config)
    {
        return new WorldConfigModel
        {
            WorldName = config.WorldName,
            TickInterval = config.TickInterval
        };
    }
}