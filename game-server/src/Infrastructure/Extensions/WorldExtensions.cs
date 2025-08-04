using Villagers.GameServer.Configuration;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Entities;
using Villagers.GameServer.Extensions;

namespace Villagers.GameServer.Infrastructure.Extensions;

public static class WorldExtensions
{
    public static WorldEntity ToEntity(this World world)
    {
        return new WorldEntity
        {
            Id = world.Id,
            TickNumber = world.TickNumber,
            LastUpdated = DateTime.UtcNow
        };
    }

    public static World ToDomain(this WorldEntity entity, CommandQueue commandQueue, WorldConfiguration config)
    {
        var domainConfig = config.ToDomain();
        return new World(entity.Id, domainConfig, commandQueue, (int)entity.TickNumber);
    }
}