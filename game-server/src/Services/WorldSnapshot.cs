using Villagers.GameServer.Domain;

namespace Villagers.GameServer.Services;

/// <summary>
/// Immutable snapshot containing only the data needed for world persistence.
/// This prevents race conditions by capturing world state at a specific point in time.
/// </summary>
public class WorldSnapshot
{
    public Guid Id { get; }
    public int TickNumber { get; }
    public WorldConfig Config { get; }

    public WorldSnapshot(World world)
    {
        Id = world.Id;
        TickNumber = world.GetCurrentTickNumber();
        Config = world.Config ?? throw new ArgumentNullException(nameof(world.Config));
    }
}