using Villagers.GameServer.Domain;

namespace Villagers.GameServer.Infrastructure.Repositories;

public interface IRegistrationIntentRepository
{
    Task<RegistrationIntent> CreateIntentAsync(RegistrationIntent intent);
    Task<RegistrationIntent?> GetPendingIntentAsync(Guid playerId);
    Task<List<RegistrationIntent>> GetAllPendingIntentsAsync();
    Task SaveIntentAsync(RegistrationIntent intent);
    Task DeleteIntentAsync(Guid intentId);
}