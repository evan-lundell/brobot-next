using System.Security.Claims;
using Brobot.Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using ClaimTypes = Brobot.Shared.Claims.ClaimTypes;

namespace Brobot.Frontend.Providers;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState NotAuthenticatedState = new (new ClaimsPrincipal());

    private LoginUser? _user;
    private readonly JwtService _jwtService;
    private readonly IServiceProvider _services;

    public string? DisplayName => _user?.DisplayName;
    public bool IsLoggedIn => _user != null;
    public string? Token => _user?.Jwt;
    private bool IsDiscordAuthenticated => _user != null && !string.IsNullOrWhiteSpace(_user.Principal.FindFirst(ClaimTypes.DiscordId)?.Value);


    public JwtAuthenticationStateProvider(JwtService jwtService, IServiceProvider services)
    {
        _jwtService = jwtService;
        _services = services;
    }

    public void Login(string jwt)
    {
        var principal = _jwtService.Deserialize(jwt);
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
        using var scope = _services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<ApiService>();
        await api.Logout();
        _user = null;
        NotifyAuthenticationStateChanged(Task.FromResult(GetState()));
    }

    private AuthenticationState GetState()
    {
        if (IsLoggedIn && IsDiscordAuthenticated)
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