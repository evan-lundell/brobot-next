using System.Text;
using System.Text.Json;

namespace Brobot.Frontend.Tests.Infrastructure;

/// <summary>
/// Builds unsigned JWTs (header.payload.signature) whose payload is an arbitrary claim set,
/// for exercising <c>JwtService.Deserialize</c>. The signature is not validated by the client,
/// so a placeholder is used.
/// </summary>
public static class TestJwt
{
    public static string Create(object claims)
    {
        var header = Base64UrlEncode("""{"alg":"none","typ":"JWT"}"""u8.ToArray());
        var payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(claims));
        return $"{header}.{payload}.signature";
    }

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
