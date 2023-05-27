using System.Security.Claims;

namespace Brobot.Frontend;

public class LoginUser
{
    public string? DisplayName { get; init; }
    public required string Jwt { get; init; }
    public required ClaimsPrincipal Principal { get; init; }
}