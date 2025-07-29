using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Villagers.Api.Controllers.Requests;
using Villagers.Api.Services;
using Xunit;

namespace Villagers.Api.Tests.Services;

public class CommandServiceTests
{
    private readonly Mock<ILogger<CommandService>> _loggerMock;
    private readonly IConfiguration _configuration;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly CommandService _service;

    public CommandServiceTests()
    {
        _loggerMock = new Mock<ILogger<CommandService>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        // Use real configuration with in-memory values
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GameServer:Url"] = "http://localhost:5033"
        });
        _configuration = configBuilder.Build();

        _service = new CommandService(_httpClient, _loggerMock.Object, _configuration);
    }

    [Fact]
    public async Task SendTestCommandAsync_WithSuccessfulResponse_ShouldReturnTrue()
    {
        // Arrange
        var request = new TestCommandRequest
        {
            PlayerId = "player123",
            Message = "Test message"
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"status\":\"ok\"}", Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.SendTestCommandAsync(request);

        // Assert
        result.Should().BeTrue();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Test command sent successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendTestCommandAsync_WithFailedResponse_ShouldReturnFalse()
    {
        // Arrange
        var request = new TestCommandRequest
        {
            PlayerId = "player456",
            Message = "Another test message"
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.SendTestCommandAsync(request);

        // Assert
        result.Should().BeFalse();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Failed to send test command")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendTestCommandAsync_WithException_ShouldReturnFalse()
    {
        // Arrange
        var request = new TestCommandRequest
        {
            PlayerId = "player789",
            Message = "Exception test message"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.SendTestCommandAsync(request);

        // Assert
        result.Should().BeFalse();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error sending test command")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendTestCommandAsync_ShouldSendCorrectRequestContent()
    {
        // Arrange
        var request = new TestCommandRequest
        {
            PlayerId = "player999",
            Message = "Content test message"
        };

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        await _service.SendTestCommandAsync(request);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.RequestUri!.ToString().Should().Be("http://localhost:5033/api/command/test");
        
        var content = await capturedRequest.Content!.ReadAsStringAsync();
        var deserializedContent = JsonSerializer.Deserialize<TestCommandRequest>(content);
        deserializedContent.Should().BeEquivalentTo(request);
    }

    [Fact]
    public async Task SendTestCommandAsync_WithCustomGameServerUrl_ShouldUseConfiguredUrl()
    {
        // Arrange
        var customUrl = "http://custom-server:8080";
        var customConfigBuilder = new ConfigurationBuilder();
        customConfigBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GameServer:Url"] = customUrl
        });
        var customConfig = customConfigBuilder.Build();

        var service = new CommandService(_httpClient, _loggerMock.Object, customConfig);

        var request = new TestCommandRequest
        {
            PlayerId = "player111",
            Message = "Custom URL test"
        };

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        await service.SendTestCommandAsync(request);

        // Assert
        capturedRequest!.RequestUri!.ToString().Should().Be($"{customUrl}/api/command/test");
    }
}