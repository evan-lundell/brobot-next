using System.Security.Claims;
using Brobot.Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace Brobot.Frontend.Providers;

public class JwtAuthenticationStateProvider(JwtService jwtService, IServiceScopeFactory serviceScopeFactory)
    : AuthenticationStateProvider
{
    private static readonly AuthenticationState NotAuthenticatedState = new (new ClaimsPrincipal());

    private LoginUser? _user;

    public string? DisplayName => _user?.DisplayName;
    public bool IsLoggedIn => _user != null;
    public string? Token => _user?.Jwt;


    public void Login(string jwt)
    {
        var principal = jwtService.Deserialize(jwt);
        _user = new LoginUser
        {
            DisplayName = principal.Identity?.Name ?? "",
            Jwt = jwt,
            Principal = principal
        };
        NotifyAuthenticationStateChanged(Task.FromResult(GetState()));
    }

    public async Task Logout()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<ApiService>();
        await api.Logout();
        _user = null;
        NotifyAuthenticationStateChanged(Task.FromResult(GetState()));
    }

    private AuthenticationState GetState()
    {
        if (IsLoggedIn)
        {
            return new AuthenticationState(_user!.Principal);
        }
        return NotAuthenticatedState;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(GetState());
    }
}