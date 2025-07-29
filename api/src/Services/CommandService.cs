using System.Text;
using System.Text.Json;
using Villagers.Api.Controllers.Requests;

namespace Villagers.Api.Services;

public class CommandService : ICommandService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CommandService> _logger;
    private readonly string _gameServerUrl;

    public CommandService(HttpClient httpClient, ILogger<CommandService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _gameServerUrl = configuration.GetValue<string>("GameServer:Url") ?? "http://localhost:5033";
    }

    public async Task<bool> SendTestCommandAsync(TestCommandRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_gameServerUrl}/api/command/test", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Test command sent successfully to game server for player {PlayerId}", request.PlayerId);
                return true;
            }
            
            _logger.LogWarning("Failed to send test command to game server. Status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test command to game server");
            return false;
        }
    }


}