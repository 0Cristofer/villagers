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
            RetryCount = intent.RetryCount,
            IsCompleted = intent.IsCompleted,
            LastError = intent.LastError
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
            entity.IsCompleted,
            entity.LastError
        );
    }
}