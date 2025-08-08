using Villagers.GameServer.Domain;
using Villagers.GameServer.Domain.Enums;

namespace Villagers.GameServer.Services;

public interface IPlayerRegistrationIntentService
{
    Task<RegistrationIntent> CreateRegistrationIntentAsync(Guid playerId, StartingDirection startingDirection);
    Task<RegistrationIntent?> GetPendingIntentAsync(Guid playerId);
    Task ProcessRegistrationAsync(RegistrationIntent intent);
    Task<List<RegistrationIntent>> GetAllPendingIntentsAsync();
}