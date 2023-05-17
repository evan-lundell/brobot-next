using System.Security.Claims;

namespace Brobot.Frontend;

public class LoginUser
{
    public string? DisplayName { get; set; }
    public required string Jwt { get; set; }
    public required ClaimsPrincipal Principal { get; set; }
}