// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Villagers.GameServer.Models;

public class WorldConfigModel
{
    public string WorldName { get; set; } = string.Empty;
    public TimeSpan TickInterval { get; set; }
}