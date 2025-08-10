namespace Villagers.Api.Domain;

public class AuthenticatedPlayer
{
    public Player Player { get; }
    public PlayerToken Token { get; }

    public AuthenticatedPlayer(Player player, PlayerToken token)
    {
        Player = player;
        Token = token;
    }
}