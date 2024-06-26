using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Brobot.Frontend.Providers;
using Microsoft.AspNetCore.Components;

namespace Brobot.Frontend.Services;

public class JwtService
{
    private readonly IServiceProvider _services;
    private readonly NavigationManager _navigationManager;

    public JwtService(IServiceProvider services, NavigationManager navigationManager)
    {
        _services = services;
        _navigationManager = navigationManager;
    }

    public async Task RefreshJwtToken()
    {
        var api = _services.GetRequiredService<ApiService>();
        var loginResponse = await api.RefreshToken();
        if (loginResponse is { Succeeded: true } && !string.IsNullOrWhiteSpace(loginResponse.Token))
        {
            using var scope = _services.CreateScope();
            var loginStateService = scope.ServiceProvider.GetRequiredService<JwtAuthenticationStateProvider>();
            loginStateService.Login(loginResponse.Token);
        }
        else
        {
            _navigationManager.NavigateTo("/");
        }
    }

    public ClaimsPrincipal Deserialize(string jwtToken)
    {
        var segments = jwtToken.Split('.');

        if (segments.Length != 3)
        {
            throw new Exception("Invalid JWT");
        }

        Console.WriteLine(segments[1]);
        var dataSegment = Encoding.UTF8.GetString(FromUrlBase64(segments[1]));
        var data = JsonSerializer.Deserialize<JsonObject>(dataSegment);

        if (data == null)
        {
            throw new Exception("Unable to deserialize jwt");
        }

        var claims = new Claim[data.Count];
        int index = 0;
        foreach (var entry in data)
        {
            if (entry.Value == null)
            {
                continue;
            }
            claims[index] = JwtNodeToClaim(entry.Key, entry.Value);
            index++;
        }

        var claimIdentity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(new[] { claimIdentity });

        return principal;
    }

    private Claim JwtNodeToClaim(string key, JsonNode node)
    {
        var jsonValue = node.AsValue();

        if (jsonValue.TryGetValue<string>(out var str))
        {
            return new Claim(key, str, ClaimValueTypes.String);
        }

        if (jsonValue.TryGetValue<double>(out var num))
        {
            return new Claim(key, num.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Double);
        }

        throw new Exception("Unsupported JWT claim type");
    }

    private byte[] FromUrlBase64(string jwtSegment)
    {
        string fixedBase64 = jwtSegment
            .Replace('-', '+')
            .Replace('_', '/');

        fixedBase64 += (jwtSegment.Length % 4) switch
        {
            0 => "",
            2 => "==",
            3 => "=",
            _ => throw new Exception("Illegal base64url string!")
        };

        return Convert.FromBase64String(fixedBase64);
    }
}