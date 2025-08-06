using Microsoft.EntityFrameworkCore;

namespace Villagers.GameServer.Entities;

[Owned]
public class WorldConfigEntity
{
    public string WorldName { get; set; } = string.Empty;
    public long TickIntervalMs { get; set; } = 1000; // Store TimeSpan as milliseconds
}