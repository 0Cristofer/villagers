using Villagers.GameServer.Configuration;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Entities;
using Villagers.GameServer.Extensions;
using Villagers.GameServer.Services;

namespace Villagers.GameServer.Infrastructure.Extensions;

public static class WorldExtensions
{
    public static WorldEntity ToEntity(this World world)
    {
        return new WorldEntity
        {
            Id = world.Id,
            TickNumber = world.GetCurrentTickNumber(),
            Config = world.Config.ToEntity(),
            LastUpdated = DateTime.UtcNow
        };
    }

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
        // Use the persisted configuration to ensure simulation consistency
        var domainConfig = entity.Config.ToDomain();
        var commandQueue = new CommandQueue();
        return new World(entity.Id, domainConfig, commandQueue, (int)entity.TickNumber);
    }
}