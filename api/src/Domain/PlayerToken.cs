namespace Villagers.Api.Domain;

public class PlayerToken
{
    public string Token { get; }
    public DateTime ExpiresAt { get; }

    public PlayerToken(string token, DateTime expiresAt)
    {
        Token = token;
        ExpiresAt = expiresAt;
    }
}