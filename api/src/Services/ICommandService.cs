using Villagers.Api.Controllers.Requests;

namespace Villagers.Api.Services;

public interface ICommandService
{
    Task<bool> SendTestCommandAsync(TestCommandRequest request);
}