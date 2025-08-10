using Villagers.Api.Domain;

namespace Villagers.Api.Services;

public interface IJwtService
{
    PlayerToken GenerateToken(Player player);
}