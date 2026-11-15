using System.Net;
using System.Text;
using Brobot.Frontend.Services;

namespace Brobot.Tests.FrontendTests;

[TestFixture]
public class ApiServiceTests
{
    private static readonly ulong[] ExpectedChannelIds = [100];
    private static readonly string[] ExpectedUsernames = ["User1"];

    private HttpRequestMessage? _lastRequest;

    [TestCase(null, "http://localhost/Channels", TestName = "GetChannels_NoUserId_RequestsCallersChannels")]
    [TestCase(2UL, "http://localhost/Channels?userId=2", TestName = "GetChannels_WithUserId_RequestsThatUsersChannels")]
    public async Task GetChannels_RequestsExpectedUrl(ulong? userId, string expectedUrl)
    {
        var apiService = ApiService(Response(HttpStatusCode.OK, "[{\"id\":100,\"name\":\"Channel\"}]", "application/json"));

        var channels = await apiService.GetChannels(userId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_lastRequest?.RequestUri?.ToString(), Is.EqualTo(expectedUrl));
            Assert.That(channels.Select(c => c.Id), Is.EqualTo(ExpectedChannelIds));
        }
    }

    [Test]
    public async Task GetChannels_Forbidden_ThrowsFriendlyMessage()
    {
        var apiService = ApiService(new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("") });

        var exception = await Assert.ThrowsAsync<Exception>(() => apiService.GetChannels(2));

        Assert.That(exception!.Message, Is.EqualTo("You don't have permission to do that"));
    }

    [Test]
    public async Task GetDiscordUsers_Forbidden_ThrowsFriendlyMessage()
    {
        var apiService = ApiService(new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("") });

        var exception = await Assert.ThrowsAsync<Exception>(apiService.GetDiscordUsers);

        Assert.That(exception!.Message, Is.EqualTo("You don't have permission to do that"));
    }

    [Test]
    public async Task GetDiscordUsers_Success_ReturnsUsers()
    {
        var apiService = ApiService(Response(HttpStatusCode.OK, "[{\"id\":1,\"username\":\"User1\"}]", "application/json"));

        var users = await apiService.GetDiscordUsers();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_lastRequest?.RequestUri?.ToString(), Is.EqualTo("http://localhost/Users/all"));
            Assert.That(users.Select(u => u.Username), Is.EqualTo(ExpectedUsernames));
        }
    }

    private ApiService ApiService(HttpResponseMessage response)
        => new(new HttpClient(new StubHandler(response, request => _lastRequest = request))
        {
            BaseAddress = new Uri("http://localhost/")
        });

    private static HttpResponseMessage Response(HttpStatusCode statusCode, string body, string mediaType) => new(statusCode)
    {
        Content = new StringContent(body, Encoding.UTF8, mediaType)
    };

    private sealed class StubHandler(HttpResponseMessage response, Action<HttpRequestMessage> onRequest) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            onRequest(request);
            return Task.FromResult(response);
        }
    }
}
