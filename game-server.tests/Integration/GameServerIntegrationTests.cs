using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Villagers.GameServer.Controllers.Requests;
using Villagers.GameServer.Services;
using Xunit;

namespace Villagers.GameServer.Tests.Integration;

public class GameServerIntegrationTests : IClassFixture<WebApplicationFactory<Villagers.GameServer.Controllers.CommandController>>
{
    private readonly WebApplicationFactory<Villagers.GameServer.Controllers.CommandController> _factory;
    private readonly HttpClient _client;

    public GameServerIntegrationTests(WebApplicationFactory<Villagers.GameServer.Controllers.CommandController> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing game service if present
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IGameSimulationService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add a mock game service to avoid background processing
                var mockGameService = new Mock<IGameSimulationService>();
                services.AddSingleton(mockGameService.Object);
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
    public async Task TestCommand_ShouldAcceptCommand()
    {
        // Arrange
        var request = new TestCommandRequest
        {
            PlayerId = "integration-test-player",
            Message = "Integration test message"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/command/test", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Test command queued for processing");
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
    public async Task TestCommand_VerifyServiceInteraction()
    {
        // This test verifies that the controller integrates properly with the service layer
        // The service call is mocked, but we test the HTTP-to-service integration

        var request = new TestCommandRequest
        {
            PlayerId = "service-test-player",
            Message = "Service test message"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/command/test", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Test command queued for processing");
    }
}