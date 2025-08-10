namespace Villagers.Api.Domain;

public class Player
{
    public Guid Id { get; }
    public string Username { get; private set; }
    public List<Guid> RegisteredWorldIds { get; }
    public DateTime CreatedAt { get; }
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
        RegisteredWorldIds = registeredWorldIds;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public void RegisterForWorld(Guid worldId)
    {
        if (RegisteredWorldIds.Contains(worldId))
            return;
        
        RegisteredWorldIds.Add(worldId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnregisterFromWorld(Guid worldId)
    {
        if (!RegisteredWorldIds.Remove(worldId))
            return;
        
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsRegisteredForWorld(Guid worldId)
    {
        return RegisteredWorldIds.Contains(worldId);
    }
}