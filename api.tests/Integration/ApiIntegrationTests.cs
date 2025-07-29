using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Villagers.Api.Controllers.Requests;
using Villagers.Api.Services;
using Xunit;

namespace Villagers.Api.Tests.Integration;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Villagers.Api.Controllers.CommandController>>
{
    private readonly WebApplicationFactory<Villagers.Api.Controllers.CommandController> _factory;
    private readonly HttpClient _client;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

    public ApiIntegrationTests(WebApplicationFactory<Villagers.Api.Controllers.CommandController> factory)
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing HttpClient and CommandService
                var httpClientDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(HttpClient));
                if (httpClientDescriptor != null)
                {
                    services.Remove(httpClientDescriptor);
                }
                
                var commandServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICommandService));
                if (commandServiceDescriptor != null)
                {
                    services.Remove(commandServiceDescriptor);
                }

                // Add configuration
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["GameServer:Url"] = "http://localhost:5033"
                    })
                    .Build();
                services.AddSingleton<IConfiguration>(configuration);

                // Add HttpClient with mocked handler
                services.AddSingleton(new HttpClient(_mockHttpMessageHandler.Object));
                services.AddScoped<ICommandService, CommandService>();
            });
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Health_ShouldReturnHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/command/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
        content.Should().Contain("timestamp");
    }

    [Fact]
    public async Task TestCommand_WithSuccessfulGameServerResponse_ShouldReturnOk()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}", Encoding.UTF8, "application/json")
            });

        var request = new TestCommandRequest
        {
            PlayerId = "integration-test-player",
            Message = "Integration test message"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/command/test", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Command sent to game server");
    }

    [Fact]
    public async Task TestCommand_WithFailedGameServerResponse_ShouldReturn500()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var request = new TestCommandRequest
        {
            PlayerId = "integration-test-player",
            Message = "Integration test message"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/command/test", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Failed to send command to game server");
    }

    [Fact]
    public async Task TestCommand_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/command/test", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TestCommand_WithGameServerTimeout_ShouldReturn500()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        var request = new TestCommandRequest
        {
            PlayerId = "timeout-test-player",
            Message = "Timeout test message"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/command/test", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Failed to send command to game server");
    }
}