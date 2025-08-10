using Villagers.Api.Domain;

namespace Villagers.Api.Services;

public interface IPlayerAuthenticationService
{
    Task<AuthenticatedPlayer> RegisterAsync(string username, string password);
    Task<AuthenticatedPlayer?> LoginAsync(string username, string password);
    Task<AuthenticatedPlayer?> RefreshTokenAsync(Guid playerId);
}