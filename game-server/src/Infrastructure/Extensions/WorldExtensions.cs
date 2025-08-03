using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Entities;

namespace Villagers.GameServer.Infrastructure.Extensions;

public static class WorldExtensions
{
    public static WorldEntity ToEntity(this World world)
    {
        return new WorldEntity
        {
            Name = world.Name,
            TickNumber = world.TickNumber,
            LastUpdated = DateTime.UtcNow
        };
    }

    public static World ToDomain(this WorldEntity entity, CommandQueue commandQueue)
    {
        return new World(entity.Name, TimeSpan.FromSeconds(1), commandQueue, (int)entity.TickNumber);
    }
}