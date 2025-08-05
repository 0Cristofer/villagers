using System.Text.Json;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Entities;

namespace Villagers.GameServer.Infrastructure.Extensions;

public static class CommandExtensions
{
    public static CommandEntity ToEntity(this ICommand command)
    {
        return new CommandEntity
        {
            Type = command.GetType().Name,
            PlayerId = command.PlayerId,
            TickNumber = command.TickNumber,
            Payload = JsonSerializer.Serialize(command),
            CreatedAt = command.Timestamp
        };
    }

    public static ICommand? ToDomain(this CommandEntity entity)
    {
        return entity.Type switch
        {
            nameof(TestCommand) => JsonSerializer.Deserialize<TestCommand>(entity.Payload),
            _ => null
        };
    }
}