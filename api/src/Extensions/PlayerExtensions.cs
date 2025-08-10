using Villagers.Api.Entities;
using Villagers.Api.Models;

namespace Villagers.Api.Extensions;

public static class PlayerExtensions
{
    public static PlayerModel ToModel(this Domain.Player player)
    {
        return new PlayerModel
        {
            Id = player.Id,
            Username = player.Username,
            RegisteredWorldIds = player.RegisteredWorldIds
        };
    }

    public static Domain.Player ToDomain(this PlayerEntity entity)
    {
        return new Domain.Player(
            entity.Id, 
            entity.UserName ?? string.Empty, 
            entity.RegisteredWorldIds, 
            entity.CreatedAt, 
            entity.UpdatedAt);
    }

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