using Villagers.GameServer.Domain;
using Villagers.GameServer.Extensions;
using Villagers.GameServer.Models;

namespace Villagers.GameServer.Services;

public class WorldRegistrationService : IWorldRegistrationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WorldRegistrationService> _logger;

    public WorldRegistrationService(HttpClient httpClient, IConfiguration configuration, ILogger<WorldRegistrationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task RegisterWorldAsync(World world)
    {
        var apiBaseUrl = _configuration["Api:BaseUrl"];
        var apiKey = _configuration["Api:ApiKey"];
        
        if (string.IsNullOrEmpty(apiBaseUrl))
            throw new InvalidOperationException("Api:BaseUrl configuration is required for world registration");
        
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("Api:ApiKey configuration is required for world registration");

        // Get server endpoint from configuration (can be overridden by environment variables)
        var serverEndpoint = _configuration["Server:Endpoint"];
        var hubPath = _configuration["Server:HubPath"] ?? "/gamehub";
        
        if (string.IsNullOrEmpty(serverEndpoint))
            throw new InvalidOperationException("Server:Endpoint configuration is required for world registration");

        var fullEndpoint = serverEndpoint + hubPath;

        var request = new RegisterWorldRequest
        {
            WorldId = world.Id,
            ServerEndpoint = fullEndpoint,
            Config = world.Config.ToModel()
        };

        _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        
        var response = await _httpClient.PostAsJsonAsync($"{apiBaseUrl}/api/internal/worlds/register", request);
        response.EnsureSuccessStatusCode(); // Let exceptions break execution

        _logger.LogInformation("Successfully registered world with API. World ID: {WorldId}, Endpoint: {Endpoint}", 
            world.Id, fullEndpoint);
    }

    public async Task UnregisterWorldAsync(World world)
    {
        var apiBaseUrl = _configuration["Api:BaseUrl"];
        var apiKey = _configuration["Api:ApiKey"];
        
        if (string.IsNullOrEmpty(apiBaseUrl) || string.IsNullOrEmpty(apiKey))
            return;

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            
            await _httpClient.DeleteAsync($"{apiBaseUrl}/api/internal/worlds/{world.Id}");
            _logger.LogInformation("Unregistered world from API");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering world from API");
        }
    }

}