using Villagers.Api.Domain;
using Villagers.Api.Entities;

namespace Villagers.Api.Extensions;

public static class WorldConfigEntityExtensions
{
    public static WorldConfigEntity ToEntity(this WorldConfig domain)
    {
        return new WorldConfigEntity
        {
            WorldName = domain.WorldName,
            TickInterval = domain.TickInterval
        };
    }

    public static WorldConfig ToDomain(this WorldConfigEntity entity)
    {
        return new WorldConfig(entity.WorldName, entity.TickInterval);
    }
}