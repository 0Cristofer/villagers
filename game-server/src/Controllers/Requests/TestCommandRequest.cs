namespace Villagers.GameServer.Controllers.Requests;

public class TestCommandRequest
{
    public string PlayerId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}