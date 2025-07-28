using System.Text;
using System.Text.Json;

namespace Villagers.Api.Services;

public class GameServerService : IGameServerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GameServerService> _logger;
    private readonly string _gameServerUrl;

    public GameServerService(HttpClient httpClient, ILogger<GameServerService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _gameServerUrl = configuration.GetValue<string>("GameServer:Url") ?? "http://localhost:5033";
    }

    public async Task<bool> SendCommandAsync(object command)
    {
        try
        {
            var json = JsonSerializer.Serialize(command);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_gameServerUrl}/api/command/execute", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Command sent successfully to game server");
                return true;
            }
            
            _logger.LogWarning("Failed to send command to game server. Status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending command to game server");
            return false;
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_gameServerUrl}/api/command/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking game server health");
            return false;
        }
    }

    public async Task<object> LoginPlayerAsync(string username)
    {
        try
        {
            var loginRequest = new { username };
            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_gameServerUrl}/api/player/login", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var playerInfo = JsonSerializer.Deserialize<object>(responseContent);
                _logger.LogInformation("Player logged in successfully: {Username}", username);
                return playerInfo ?? new { error = "Invalid response from game server" };
            }
            
            _logger.LogWarning("Failed to login player. Status: {StatusCode}", response.StatusCode);
            throw new Exception($"Failed to login player. Status: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in player");
            throw;
        }
    }
}