namespace Villagers.GameServer.Domain.Commands.Requests;

/// <summary>
/// Wrapper for command requests loaded from persistence that contains the expected tick number
/// for replay validation. This keeps the original request pristine while providing replay context.
/// </summary>
public class ReplayableCommandRequest
{
    public ICommandRequest Request { get; }
    public long ExpectedTickNumber { get; }

    public ReplayableCommandRequest(ICommandRequest request, long expectedTickNumber)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        
        if (expectedTickNumber < 0)
            throw new ArgumentException("Expected tick number must be non-negative", nameof(expectedTickNumber));
            
        ExpectedTickNumber = expectedTickNumber;
    }
}