namespace Villagers.Api.Services;

public interface IGameServerService
{
    Task<bool> SendCommandAsync(object command);
    Task<bool> IsHealthyAsync();
    Task<object> LoginPlayerAsync(string username);
}