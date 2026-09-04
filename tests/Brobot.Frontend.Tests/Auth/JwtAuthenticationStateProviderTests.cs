using System.Security.Claims;
using Brobot.Frontend.Providers;
using Brobot.Frontend.Services;
using Brobot.Frontend.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Brobot.Frontend.Tests.Auth;

[TestFixture]
public class JwtAuthenticationStateProviderTests
{
    private static JwtAuthenticationStateProvider CreateProvider()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        return new JwtAuthenticationStateProvider(
            new JwtService(services),
            services.GetRequiredService<IServiceScopeFactory>());
    }

    [Test]
    public async Task BeforeLogin_StateIsNotAuthenticated()
    {
        var provider = CreateProvider();

        var state = await provider.GetAuthenticationStateAsync();

        Assert.That(provider.IsLoggedIn, Is.False);
        Assert.That(state.User.Identity?.IsAuthenticated, Is.Not.EqualTo(true));
    }

    [Test]
    public async Task Login_SetsAuthenticatedStateWithClaims()
    {
        var provider = CreateProvider();
        var token = TestJwt.Create(new Dictionary<string, object>
        {
            [ClaimTypes.Name] = "evan",
            [ClaimTypes.Role] = "Admin"
        });

        provider.Login(token);
        var state = await provider.GetAuthenticationStateAsync();

        Assert.That(provider.IsLoggedIn, Is.True);
        Assert.That(provider.Token, Is.EqualTo(token));
        Assert.That(state.User.Identity?.IsAuthenticated, Is.True);
        Assert.That(state.User.IsInRole("Admin"), Is.True);
    }

    [Test]
    public void Login_ExposesDisplayNameFromNameClaim()
    {
        var provider = CreateProvider();
        var token = TestJwt.Create(new Dictionary<string, object> { [ClaimTypes.Name] = "evan" });

        provider.Login(token);

        Assert.That(provider.DisplayName, Is.EqualTo("evan"));
    }
}
