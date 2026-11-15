using System.Net;
using System.Text;
using Brobot.Frontend.Handlers;
using Brobot.Frontend.Providers;
using Brobot.Frontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Brobot.Tests.FrontendTests;

[TestFixture]
public class JwtTokenMessageHandlerTests
{
    private static readonly Uri ApiBase = new("http://localhost/");

    [Test]
    public async Task ServerError_PlainText_ThrowsWithBody()
    {
        using var client = Client(Response(HttpStatusCode.InternalServerError, "Database is unavailable", "text/plain"));

        var exception = await Assert.ThrowsAsync<Exception>(() => client.GetAsync("api/HotOps"));

        Assert.That(exception!.Message, Is.EqualTo("Database is unavailable"));
    }

    [Test]
    public async Task ServerError_ProblemDetails_ThrowsWithTitle()
    {
        using var client = Client(Response(HttpStatusCode.InternalServerError, "{\"type\":\"error\",\"title\":\"An error occurred\"}", "application/problem+json"));

        var exception = await Assert.ThrowsAsync<Exception>(() => client.GetAsync("api/HotOps"));

        Assert.That(exception!.Message, Is.EqualTo("An error occurred"));
    }

    [TestCase("", "text/plain", TestName = "ServerError_EmptyBody_ThrowsGenericMessage")]
    [TestCase("<html>Internal Server Error</html>", "text/html", TestName = "ServerError_Html_ThrowsGenericMessage")]
    public async Task ServerError_NothingUsable_ThrowsGenericMessage(string body, string mediaType)
    {
        using var client = Client(Response(HttpStatusCode.InternalServerError, body, mediaType));

        var exception = await Assert.ThrowsAsync<Exception>(() => client.GetAsync("api/HotOps"));

        Assert.That(exception!.Message, Is.EqualTo("Something went wrong"));
    }

    [Test]
    public async Task OtherErrors_AreReturnedForTheCallerToHandle()
    {
        using var client = Client(Response(HttpStatusCode.BadRequest, "Invalid channel", "text/plain"));

        using var response = await client.GetAsync("api/HotOps");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private static HttpClient Client(HttpResponseMessage response)
    {
        var loginState = new JwtAuthenticationStateProvider(
            new JwtService(Mock.Of<IServiceProvider>()),
            Mock.Of<IServiceScopeFactory>());
        var handler = new JwtTokenMessageHandler(ApiBase, loginState)
        {
            InnerHandler = new StubHandler(response)
        };
        return new HttpClient(handler) { BaseAddress = ApiBase };
    }

    private static HttpResponseMessage Response(HttpStatusCode statusCode, string body, string mediaType) => new(statusCode)
    {
        Content = new StringContent(body, Encoding.UTF8, mediaType)
    };

    private sealed class StubHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(response);
    }
}
