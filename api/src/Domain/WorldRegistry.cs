namespace Villagers.Api.Domain;

public class WorldRegistry
{
    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public string ServerEndpoint { get; private set; }
    public WorldConfig Config { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    public WorldRegistry(Guid id, Guid worldId, string serverEndpoint, WorldConfig config, DateTime registeredAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("WorldRegistry ID cannot be empty", nameof(id));
        if (worldId == Guid.Empty)
            throw new ArgumentException("World ID cannot be empty", nameof(worldId));
        if (string.IsNullOrWhiteSpace(serverEndpoint))
            throw new ArgumentException("Server endpoint cannot be empty", nameof(serverEndpoint));
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        Id = id;
        WorldId = worldId;
        ServerEndpoint = serverEndpoint;
        Config = config;
        RegisteredAt = registeredAt;
    }

    public WorldRegistry(Guid worldId, string serverEndpoint, WorldConfig config)
        : this(Guid.NewGuid(), worldId, serverEndpoint, config, DateTime.UtcNow)
    {
    }
}