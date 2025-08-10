using Villagers.GameServer.Domain;

namespace Villagers.GameServer.Entities;

public class RegistrationResultEntity
{
    public bool IsSuccess { get; set; }
    public RegistrationFailureReason FailureReason { get; set; }
    public string? ErrorMessage { get; set; }

    // Parameterless constructor for EF Core
    public RegistrationResultEntity()
    {
    }

    public RegistrationResultEntity(bool isSuccess, RegistrationFailureReason failureReason, string? errorMessage)
    {
        IsSuccess = isSuccess;
        FailureReason = failureReason;
        ErrorMessage = errorMessage;
    }
}