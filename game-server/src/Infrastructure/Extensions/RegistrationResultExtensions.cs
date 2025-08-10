using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Entities;

namespace Villagers.GameServer.Infrastructure.Extensions;

public static class RegistrationResultExtensions
{
    public static RegistrationResultEntity ToEntity(this RegistrationResult result)
    {
        return new RegistrationResultEntity(result.IsSuccess, result.FailureReason, result.ErrorMessage);
    }

    public static RegistrationResult ToDomain(this RegistrationResultEntity entity)
    {
        return entity.FailureReason switch
        {
            RegistrationFailureReason.None => RegistrationResult.Success(new CompletedCommand()),
            RegistrationFailureReason.GameCommandEnqueueFailed => RegistrationResult.GameCommandFailure(entity.ErrorMessage ?? "Unknown error"),
            RegistrationFailureReason.ApiRegistrationFailed => RegistrationResult.ApiFailure(entity.ErrorMessage ?? "API failure", new CompletedCommand()),
            RegistrationFailureReason.UnknownError => RegistrationResult.UnknownFailure(entity.ErrorMessage ?? "Unknown error"),
            _ => RegistrationResult.UnknownFailure(entity.ErrorMessage ?? "Unknown error")
        };
    }
}