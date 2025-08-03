using System.Security.Claims;
using Villagers.Api.Domain;

namespace Villagers.Api.Services;

public interface IJwtService
{
    string GenerateToken(Player player);
    ClaimsPrincipal ValidateToken(string token);
    DateTime GetTokenExpiration();
}