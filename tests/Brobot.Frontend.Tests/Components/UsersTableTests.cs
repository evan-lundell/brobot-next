using System.Net;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using Brobot.Frontend.Services;
using Brobot.Frontend.Shared;
using Brobot.Frontend.Tests.Infrastructure;
using Brobot.Shared.Responses;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Brobot.Frontend.Tests.Components;

[TestFixture]
public class UsersTableTests
{
    private Bunit.TestContext _ctx = null!;
    private StubHttpMessageHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _ctx = new Bunit.TestContext();
        _handler = new StubHttpMessageHandler(Respond);
        var client = new HttpClient(_handler) { BaseAddress = new Uri("https://localhost/") };
        _ctx.Services.AddSingleton(new ApiService(client));
        _ctx.Services.AddSingleton(Mock.Of<IToastService>());
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [TearDown]
    public void TearDown()
    {
        _ctx.Dispose();
        _handler.Dispose();
    }

    private static HttpResponseMessage Respond(HttpRequestMessage request)
    {
        var json = request.RequestUri!.AbsolutePath switch
        {
            "/Users/all" => JsonSerializer.Serialize(new[]
            {
                new DiscordUserResponse
                {
                    Id = 1,
                    Username = "evan",
                    Timezone = "America/New_York",
                    LastOnline = new DateTimeOffset(2026, 1, 2, 9, 30, 0, TimeSpan.FromHours(-5))
                }
            }),
            "/Users/1/settings" => JsonSerializer.Serialize(new UserSettingsResponse
            {
                Timezone = "America/New_York"
            }),
            _ => "[]"
        };
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    [Test]
    public void RendersOneRowPerUser_WithUsernameAndTimezone()
    {
        var cut = _ctx.RenderComponent<UsersTable>();

        cut.WaitForAssertion(() =>
        {
            var rows = cut.FindAll("tbody tr");
            Assert.That(rows, Has.Count.EqualTo(1));
            Assert.That(rows[0].TextContent, Does.Contain("evan"));
            Assert.That(rows[0].TextContent, Does.Contain("America/New_York"));
        });
    }

    [Test]
    public void ClickingEdit_LoadsSettings_AndShowsModalViaJsInterop()
    {
        var cut = _ctx.RenderComponent<UsersTable>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1);

        cut.Find("tbody button").Click();

        cut.WaitForAssertion(() =>
        {
            // The edit flow fetches the user's settings...
            Assert.That(_handler.Requests.Any(r => r.RequestUri!.AbsolutePath == "/Users/1/settings"), Is.True);
            // ...and opens the Bootstrap modal through JS interop.
            _ctx.JSInterop.VerifyInvoke("showModal");
        });
    }
}
