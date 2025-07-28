namespace Villagers.GameServer.DTOs;

public class WorldStateDto
{
    public string Name { get; set; } = string.Empty;
    public int TickNumber { get; set; }
    public DateTime Timestamp { get; set; }

    public WorldStateDto()
    {
    }

    public WorldStateDto(string name, int tickNumber, DateTime timestamp)
    {
        Name = name;
        TickNumber = tickNumber;
        Timestamp = timestamp;
    }
}