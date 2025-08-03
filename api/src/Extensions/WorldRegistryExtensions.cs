using Villagers.Api.Domain;
using Villagers.Api.Entities;
using Villagers.Api.Models;

namespace Villagers.Api.Extensions;

public static class WorldRegistryExtensions
{
    public static WorldRegistryEntity ToEntity(this WorldRegistry domain)
    {
        return new WorldRegistryEntity
        {
            Id = domain.Id,
            WorldId = domain.WorldId,
            ServerEndpoint = domain.ServerEndpoint,
            Config = domain.Config.ToEntity(),
            RegisteredAt = domain.RegisteredAt
        };
    }

    public static WorldRegistry ToDomain(this WorldRegistryEntity entity)
    {
        return new WorldRegistry(
            entity.Id,
            entity.WorldId,
            entity.ServerEndpoint,
            entity.Config.ToDomain(),
            entity.RegisteredAt
        );
    }

    public static WorldResponse ToModel(this WorldRegistry domain)
    {
        return new WorldResponse
        {
            Id = domain.Id,
            WorldId = domain.WorldId,
            ServerEndpoint = domain.ServerEndpoint,
            Config = domain.Config.ToModel(),
            RegisteredAt = domain.RegisteredAt
        };
    }
}