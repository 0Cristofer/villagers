using Villagers.GameServer.Domain;
using Villagers.GameServer.Entities;
using Villagers.GameServer.Services;

namespace Villagers.GameServer.Infrastructure.Extensions;

public static class WorldExtensions
{
    public static WorldEntity ToEntity(this WorldSnapshot worldSnapshot)
    {
        return new WorldEntity
        {
            Id = worldSnapshot.Id,
            TickNumber = worldSnapshot.TickNumber,
            Config = worldSnapshot.Config.ToEntity(),
            LastUpdated = DateTime.UtcNow
        };
    }

    public static World ToDomain(this WorldEntity entity)
    {
        var domainConfig = entity.Config.ToDomain();
        return new World(entity.Id, domainConfig, entity.TickNumber);
    }
}