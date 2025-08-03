namespace Villagers.Api.Models;

public class WorldResponse
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }
    public string ServerEndpoint { get; set; } = string.Empty;
    public WorldConfigModel Config { get; set; } = null!;
    public DateTime RegisteredAt { get; set; }
}