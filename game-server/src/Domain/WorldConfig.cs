namespace Villagers.GameServer.Domain;

public class WorldConfig
{
    public string WorldName { get; private set; }
    public TimeSpan TickInterval { get; private set; }

    public WorldConfig(string worldName, TimeSpan tickInterval)
    {
        if (string.IsNullOrWhiteSpace(worldName))
            throw new ArgumentException("World name cannot be empty", nameof(worldName));
        if (tickInterval <= TimeSpan.Zero)
            throw new ArgumentException("Tick interval must be positive", nameof(tickInterval));

        WorldName = worldName;
        TickInterval = tickInterval;
    }
}