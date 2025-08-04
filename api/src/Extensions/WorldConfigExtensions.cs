using Villagers.Api.Domain;
using Villagers.Api.Models;

namespace Villagers.Api.Extensions;

public static class WorldConfigExtensions
{
    public static WorldConfig ToDomain(this WorldConfigModel model)
    {
        return new WorldConfig(model.WorldName, model.TickInterval);
    }

    public static WorldConfigModel ToModel(this WorldConfig domain)
    {
        return new WorldConfigModel
        {
            WorldName = domain.WorldName,
            TickInterval = domain.TickInterval
        };
    }

    public static WorldRegistry ToDomain(this RegisterWorldRequest request)
    {
        return new WorldRegistry(
            request.WorldId,
            request.ServerEndpoint,
            request.Config.ToDomain());
    }
}