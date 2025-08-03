using Villagers.Api.Entities;
using Villagers.Api.Models;

namespace Villagers.Api.Extensions;

public static class PlayerExtensions
{
    // Domain to Model (ONLY allowed conversion to model)
    public static PlayerModel ToModel(this Domain.Player player)
    {
        return new PlayerModel
        {
            Id = player.Id,
            Username = player.Username,
            RegisteredWorldIds = player.RegisteredWorldIds
        };
    }

    // Entity to Domain
    public static Domain.Player ToDomain(this PlayerEntity entity)
    {
        return new Domain.Player(
            entity.Id, 
            entity.UserName ?? string.Empty, 
            entity.RegisteredWorldIds, 
            entity.CreatedAt, 
            entity.UpdatedAt);
    }

    // Domain to Entity (for persistence)
    public static PlayerEntity ToEntity(this Domain.Player player)
    {
        return new PlayerEntity
        {
            Id = player.Id,
            UserName = player.Username,
            RegisteredWorldIds = player.RegisteredWorldIds.ToList(),
            UpdatedAt = player.UpdatedAt
        };
    }
}