using System.Net;
using System.Text.Json;
using Brobot.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace Brobot.Tests;

[TestFixture]
public class DiscordOauthServiceTests
{
    private Mock<IConfiguration> _configMock;
    private HttpClient _httpClient;
    private Mock<HttpMessageHandler> _handlerMock;

    [SetUp]
    public void SetUp()
    {
        _configMock = new Mock<IConfiguration>();
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object);

        _configMock.Setup(c => c["DiscordClientId"]).Returns("client_id");
        _configMock.Setup(c => c["DiscordClientSecret"]).Returns("client_secret");
        _configMock.Setup(c => c["DiscordTokenEndpoint"]).Returns("https://discord.com/api/oauth2/token");
        _configMock.Setup(c => c["DiscordUserInformationEndpoint"]).Returns("https://discord.com/api/users/@me");
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

        var service = new DiscordOauthService(_httpClient, _configMock.Object);
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

        var service = new DiscordOauthService(_httpClient, _configMock.Object);
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

        var service = new DiscordOauthService(new HttpClient(handler.Object), _configMock.Object);
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

        var service = new DiscordOauthService(new HttpClient(handler.Object), _configMock.Object);
        Assert.ThrowsAsync<Exception>(async () => await service.GetDiscordUserId("token"));
    }
    
    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }
}