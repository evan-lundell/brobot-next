using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Brobot.Frontend.Providers;

namespace Brobot.Frontend.Services;

public class JwtService(IServiceProvider services)
{
    public async Task RefreshJwtToken()
    {
        try
        {
            var api = services.GetRequiredService<ApiService>();
            var loginResponse = await api.RefreshToken();
            if (loginResponse is { Succeeded: true } && !string.IsNullOrWhiteSpace(loginResponse.Token))
            {
                using var scope = services.CreateScope();
                var loginStateService = scope.ServiceProvider.GetRequiredService<JwtAuthenticationStateProvider>();
                loginStateService.Login(loginResponse.Token);
            }
            // If refresh fails, user stays logged out - they can click login when ready
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token refresh failed: {ex.Message}");
            // Silently fail - user will need to re-authenticate
        }
    }

    public ClaimsPrincipal Deserialize(string jwtToken)
    {
        var segments = jwtToken.Split('.');

        if (segments.Length != 3)
        {
            throw new Exception("Invalid JWT");
        }
        
        var dataSegment = Encoding.UTF8.GetString(FromUrlBase64(segments[1]));
        var data = JsonSerializer.Deserialize<JsonObject>(dataSegment);

        if (data == null)
        {
            throw new Exception("Unable to deserialize jwt");
        }

        var claims = new List<Claim>();
        foreach (var entry in data)
        {
            if (entry.Value == null)
            {
                continue;
            }
            claims.AddRange(JwtNodeToClaims(entry.Key, entry.Value));
        }

        var claimIdentity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal([claimIdentity]);

        return principal;
    }

    private IEnumerable<Claim> JwtNodeToClaims(string key, JsonNode node)
    {
        // Handle arrays (e.g., multiple roles)
        if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item != null)
                {
                    foreach (var claim in JwtNodeToClaims(key, item))
                    {
                        yield return claim;
                    }
                }
            }
            yield break;
        }

        var jsonValue = node.AsValue();

        if (jsonValue.TryGetValue<string>(out var str))
        {
            yield return new Claim(key, str, ClaimValueTypes.String);
            yield break;
        }

        if (jsonValue.TryGetValue<double>(out var num))
        {
            yield return new Claim(key, num.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Double);
            yield break;
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