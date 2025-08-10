// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Villagers.GameServer.Models;

public class RegisterWorldRequest
{
    public Guid WorldId { get; set; }
    public string ServerEndpoint { get; set; } = string.Empty;
    public WorldConfigModel Config { get; set; } = null!;
}