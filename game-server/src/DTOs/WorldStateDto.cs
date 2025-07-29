namespace Villagers.GameServer.DTOs;

public class WorldStateDto
{
    public string Name { get; set; } = string.Empty;
    public int TickNumber { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }

    public WorldStateDto(string name, int tickNumber, string message, DateTime timestamp)
    {
        Name = name;
        TickNumber = tickNumber;
        Message = message;
        Timestamp = timestamp;
    }
}