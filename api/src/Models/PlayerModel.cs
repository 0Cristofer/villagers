namespace Villagers.Api.Models;

public class PlayerModel
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<Guid> RegisteredWorldIds { get; set; } = [];
}