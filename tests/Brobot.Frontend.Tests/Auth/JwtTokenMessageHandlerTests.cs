using System.Net;
using System.Text;
using System.Text.Json;
using Brobot.Frontend.Handlers;
using Brobot.Frontend.Providers;
using Brobot.Frontend.Services;
using Brobot.Frontend.Tests.Infrastructure;
using Brobot.Shared.Responses;
using Microsoft.Extensions.DependencyInjection;

namespace Brobot.Frontend.Tests.Auth;

[TestFixture]
public class JwtTokenMessageHandlerTests
{
    private static readonly Uri BaseAddress = new("https://brobot.example/");

    private static JwtAuthenticationStateProvider LoggedInAs(string token)
    {
        var provider = new JwtAuthenticationStateProvider(
            new JwtService(new ServiceCollection().BuildServiceProvider()),
            new ServiceCollection().BuildServiceProvider().GetRequiredService<IServiceScopeFactory>());
        provider.Login(token);
        return provider;
    }

    [Test]
    public async Task SendAsync_ToSameOrigin_AttachesBearerToken()
    {
        var inner = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var token = TestJwt.Create(new Dictionary<string, object> { ["name"] = "evan" });
        var handler = new JwtTokenMessageHandler(BaseAddress, LoggedInAs(token)) { InnerHandler = inner };
        var client = new HttpClient(handler) { BaseAddress = BaseAddress };

        await client.GetAsync("api/HotOps");

        Assert.That(inner.LastRequest.Headers.Authorization?.Scheme, Is.EqualTo("Bearer"));
        Assert.That(inner.LastRequest.Headers.Authorization?.Parameter, Is.EqualTo(token));
    }

    [Test]
    public async Task SendAsync_ToDifferentOrigin_DoesNotAttachToken()
    {
        var inner = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var token = TestJwt.Create(new Dictionary<string, object> { ["name"] = "evan" });
        var handler = new JwtTokenMessageHandler(BaseAddress, LoggedInAs(token)) { InnerHandler = inner };
        var client = new HttpClient(handler);

        await client.GetAsync("https://external.example/data");

        Assert.That(inner.LastRequest.Headers.Authorization, Is.Null);
    }

    [Test]
    public void SendAsync_On500WithErrorResponse_ThrowsWithTitle()
    {
        var error = JsonSerializer.Serialize(new ErrorResponse { Type = "error", Title = "Server exploded" });
        var inner = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(error, Encoding.UTF8, "application/json")
        });
        var token = TestJwt.Create(new Dictionary<string, object> { ["name"] = "evan" });
        var handler = new JwtTokenMessageHandler(BaseAddress, LoggedInAs(token)) { InnerHandler = inner };
        var client = new HttpClient(handler) { BaseAddress = BaseAddress };

        var ex = Assert.ThrowsAsync<Exception>(async () => await client.GetAsync("api/HotOps"));
        Assert.That(ex!.Message, Is.EqualTo("Server exploded"));
    }
}
