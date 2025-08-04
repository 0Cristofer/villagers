using Villagers.GameServer.Configuration;
using Villagers.GameServer.Domain;

namespace Villagers.GameServer.Extensions;

public static class WorldConfigurationExtensions
{
    public static WorldConfig ToDomain(this WorldConfiguration config)
    {
        return new WorldConfig(config.WorldName, config.TickInterval);
    }
}