using Microsoft.AspNetCore.Identity;
using Villagers.Api.Domain;
using Villagers.Api.Entities;
using Villagers.Api.Extensions;

namespace Villagers.Api.Services;

public class PlayerAuthenticationService : IPlayerAuthenticationService
{
    private readonly UserManager<PlayerEntity> _userManager;
    private readonly SignInManager<PlayerEntity> _signInManager;
    private readonly IJwtService _jwtService;

    public PlayerAuthenticationService(
        UserManager<PlayerEntity> userManager,
        SignInManager<PlayerEntity> signInManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    public async Task<AuthenticatedPlayer> RegisterAsync(string username, string password)
    {
        var domainPlayer = new Player(Guid.NewGuid(), username);
        
        var playerEntity = domainPlayer.ToEntity();
        var result = await _userManager.CreateAsync(playerEntity, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to register player: {errors}");
        }

        var token = _jwtService.GenerateToken(domainPlayer);
        return new AuthenticatedPlayer(domainPlayer, token);
    }

    public async Task<AuthenticatedPlayer?> LoginAsync(string username, string password)
    {
        var playerEntity = await _userManager.FindByNameAsync(username);
        if (playerEntity == null)
        {
            return null;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(playerEntity, password, false);
        if (!result.Succeeded)
        {
            return null;
        }

        var domainPlayer = playerEntity.ToDomain();
        var token = _jwtService.GenerateToken(domainPlayer);
        return new AuthenticatedPlayer(domainPlayer, token);
    }

    public async Task<AuthenticatedPlayer?> RefreshTokenAsync(Guid playerId)
    {
        var playerEntity = await _userManager.FindByIdAsync(playerId.ToString());
        if (playerEntity == null)
        {
            return null;
        }

        var domainPlayer = playerEntity.ToDomain();
        var token = _jwtService.GenerateToken(domainPlayer);
        return new AuthenticatedPlayer(domainPlayer, token);
    }
}