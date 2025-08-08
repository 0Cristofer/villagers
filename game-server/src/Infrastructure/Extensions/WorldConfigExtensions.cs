using Villagers.GameServer.Domain;
using Villagers.GameServer.Entities;

namespace Villagers.GameServer.Infrastructure.Extensions;

public static class WorldConfigExtensions
{
    public static WorldConfigEntity ToEntity(this WorldConfig config)
    {
        return new WorldConfigEntity
        {
            WorldName = config.WorldName,
            TickIntervalMs = (long)config.TickInterval.TotalMilliseconds
        };
    }

    public static WorldConfig ToDomain(this WorldConfigEntity entity)
    {
        return new WorldConfig(
            entity.WorldName,
            TimeSpan.FromMilliseconds(entity.TickIntervalMs)
        );
    }
}