using Villagers.GameServer.Domain.Commands;

namespace Villagers.GameServer.Domain;

public enum RegistrationFailureReason
{
    None = 0,
    GameCommandEnqueueFailed,
    ApiRegistrationFailed,
    UnknownError
}

public class RegistrationResult
{
    public bool IsSuccess { get; }
    public RegistrationFailureReason FailureReason { get; }
    public string? ErrorMessage { get; }
    public ICommand? Command { get; }

    private RegistrationResult(bool isSuccess, RegistrationFailureReason failureReason, string? errorMessage, ICommand? command = null)
    {
        IsSuccess = isSuccess;
        FailureReason = failureReason;
        ErrorMessage = errorMessage;
        Command = command;
    }

    public static RegistrationResult Success(ICommand command) => new(true, RegistrationFailureReason.None, null, command);
    
    public static RegistrationResult GameCommandFailure(string message) => 
        new(false, RegistrationFailureReason.GameCommandEnqueueFailed, message);
    
    public static RegistrationResult ApiFailure(string message, ICommand command) => 
        new(false, RegistrationFailureReason.ApiRegistrationFailed, message, command);
    
    public static RegistrationResult UnknownFailure(string message) => 
        new(false, RegistrationFailureReason.UnknownError, message);
}