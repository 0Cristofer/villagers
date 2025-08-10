// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Villagers.GameServer.DTOs;

public class WorldStateDto
{
    public string Name { get; set; }
    public long TickNumber { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }

    public WorldStateDto(string name, long tickNumber, string message, DateTime timestamp)
    {
        Name = name;
        TickNumber = tickNumber;
        Message = message;
        Timestamp = timestamp;
    }
}