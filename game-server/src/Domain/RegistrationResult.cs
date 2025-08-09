namespace Villagers.GameServer.Domain;

public enum RegistrationFailureReason
{
    None = 0,
    GameCommandFailed,
    ApiRegistrationFailed,
    UnknownError
}

public class RegistrationResult
{
    public bool IsSuccess { get; }
    public RegistrationFailureReason FailureReason { get; }
    public string? ErrorMessage { get; }

    private RegistrationResult(bool isSuccess, RegistrationFailureReason failureReason, string? errorMessage)
    {
        IsSuccess = isSuccess;
        FailureReason = failureReason;
        ErrorMessage = errorMessage;
    }

    public static RegistrationResult Success() => new(true, RegistrationFailureReason.None, null);
    
    public static RegistrationResult GameCommandFailure(string message) => 
        new(false, RegistrationFailureReason.GameCommandFailed, message);
    
    public static RegistrationResult ApiFailure(string message) => 
        new(false, RegistrationFailureReason.ApiRegistrationFailed, message);
    
    public static RegistrationResult UnknownFailure(string message) => 
        new(false, RegistrationFailureReason.UnknownError, message);
}