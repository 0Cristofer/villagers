using System.Text.Json;
using Villagers.GameServer.Domain.Commands.Requests;
using Villagers.GameServer.Entities;

namespace Villagers.GameServer.Infrastructure.Extensions;

public static class CommandRequestExtensions
{
    public static CommandRequestEntity ToEntity(this ICommandRequest commandRequest)
    {
        return new CommandRequestEntity
        {
            Type = commandRequest.GetType().Name,
            PlayerId = commandRequest.PlayerId,
            TickNumber = commandRequest.ProcessedTickNumber!.Value,
            Payload = JsonSerializer.Serialize(commandRequest, commandRequest.GetType()),
            CreatedAt = commandRequest.Timestamp
        };
    }

    public static ReplayableCommandRequest ToReplayableRequest(this CommandRequestEntity entity)
    {
        ICommandRequest request = entity.Type switch
        {
            nameof(TestCommandRequest) => JsonSerializer.Deserialize<TestCommandRequest>(entity.Payload)!,
            nameof(RegisterPlayerCommandRequest) => JsonSerializer.Deserialize<RegisterPlayerCommandRequest>(entity.Payload)!,
            _ => throw new InvalidOperationException($"Invalid command request type: {entity.Type}")
        };

        // Don't set the processed tick number on the request - keep it pristine
        // Instead, return a ReplayableCommandRequest with the expected tick number
        return new ReplayableCommandRequest(request, entity.TickNumber);
    }
}