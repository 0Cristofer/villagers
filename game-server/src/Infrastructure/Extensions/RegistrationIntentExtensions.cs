using Villagers.GameServer.Domain;
using Villagers.GameServer.Entities;

namespace Villagers.GameServer.Infrastructure.Extensions;

public static class RegistrationIntentExtensions
{
    public static RegistrationIntentEntity ToEntity(this RegistrationIntent intent)
    {
        return new RegistrationIntentEntity
        {
            Id = intent.Id,
            PlayerId = intent.PlayerId,
            StartingDirection = intent.StartingDirection,
            CreatedAt = intent.CreatedAt,
            LastRetryAt = intent.LastRetryAt,
            RetryCount = intent.GetRetryCount(),
            LastResult = intent.LastResult?.ToEntity()
        };
    }

    public static RegistrationIntent ToDomain(this RegistrationIntentEntity entity)
    {
        return new RegistrationIntent(
            entity.Id,
            entity.PlayerId,
            entity.StartingDirection,
            entity.CreatedAt,
            entity.LastRetryAt,
            entity.RetryCount,
            entity.LastResult?.ToDomain()
        );
    }
}