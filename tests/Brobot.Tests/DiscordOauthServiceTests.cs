using System.Net;
using System.Text.Json;
using Brobot.Configuration;
using Brobot.Services;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace Brobot.Tests;

[TestFixture]
public class DiscordOauthServiceTests
{
    private HttpClient _httpClient;
    private Mock<HttpMessageHandler> _handlerMock;
    private IOptions<DiscordOptions> _options;

    [SetUp]
    public void SetUp()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object);
        
        DiscordOptions discordOptions = new()
        {
            BrobotToken = "abc123",
            ClientId = "client_id",
            ClientSecret = "client_secret",
            AuthorizationEndpoint = "https://discord.com/oauth2/authorize",
            TokenEndpoint = "https://discord.com/api/oauth2/token",
            UserInformationEndpoint = "https://discord.com/api/users/@me"
        };
        _options = Options.Create(discordOptions);
    }

    [Test]
    public async Task GetToken_ReturnsAccessToken_WhenSuccess()
    {
        var json = JsonSerializer.Serialize(new { access_token = "abc123" });
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        
        
        var service = new DiscordOauthService(_httpClient, _options);
        var token = await service.GetToken("auth_code");
        Assert.That(token, Is.EqualTo("abc123"));
    }

    [Test]
    public void GetToken_ThrowsException_WhenNoAccessToken()
    {
        var json = JsonSerializer.Serialize(new { access_token = "" });
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        var service = new DiscordOauthService(_httpClient, _options);
        Assert.ThrowsAsync<Exception>(async () => await service.GetToken("auth_code"));
    }

    [Test]
    public async Task GetDiscordUserId_ReturnsId_WhenSuccess()
    {
        var json = JsonSerializer.Serialize(new { id = "123456789" });
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        var service = new DiscordOauthService(new HttpClient(handler.Object), _options);
        var id = await service.GetDiscordUserId("token");
        Assert.That(id, Is.EqualTo(123456789UL));
    }

    [Test]
    public void GetDiscordUserId_ThrowsException_WhenNoId()
    {
        var json = JsonSerializer.Serialize(new { id = "" });
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        var service = new DiscordOauthService(new HttpClient(handler.Object), _options);
        Assert.ThrowsAsync<Exception>(async () => await service.GetDiscordUserId("token"));
    }
    
    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }
}