namespace Villagers.GameServer.Services;

public class PlayerRegistrationService : IPlayerRegistrationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PlayerRegistrationService> _logger;

    public PlayerRegistrationService(HttpClient httpClient, IConfiguration configuration, ILogger<PlayerRegistrationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task RegisterPlayerForWorldAsync(Guid playerId, Guid worldId)
    {
        var apiBaseUrl = _configuration["Api:BaseUrl"];
        var apiKey = _configuration["Api:ApiKey"];
        
        if (string.IsNullOrEmpty(apiBaseUrl))
            throw new InvalidOperationException("Api:BaseUrl configuration is required for player registration");
        
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("Api:ApiKey configuration is required for player registration");

        var request = new { WorldId = worldId };

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        
        var response = await _httpClient.PostAsJsonAsync($"{apiBaseUrl}/api/internal/players/{playerId}/register-world", request);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Successfully registered player {PlayerId} for world {WorldId}", playerId, worldId);
    }
}