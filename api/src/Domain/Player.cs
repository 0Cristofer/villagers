namespace Villagers.Api.Domain;

public class Player
{
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public List<Guid> RegisteredWorldIds { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Player(Guid id, string username) 
        : this(id, username, [], DateTime.UtcNow, DateTime.UtcNow)
    {
    }

    public Player(Guid id, string username, List<Guid> registeredWorldIds, DateTime createdAt, DateTime updatedAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Player ID cannot be empty", nameof(id));
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));

        Id = id;
        Username = username;
        RegisteredWorldIds = registeredWorldIds ?? [];
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public void RegisterForWorld(Guid worldId)
    {
        if (!RegisteredWorldIds.Contains(worldId))
        {
            RegisteredWorldIds.Add(worldId);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UnregisterFromWorld(Guid worldId)
    {
        if (RegisteredWorldIds.Remove(worldId))
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public bool IsRegisteredForWorld(Guid worldId)
    {
        return RegisteredWorldIds.Contains(worldId);
    }
}