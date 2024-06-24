using System.Security.Claims;
using Brobot.Repositories;

namespace Brobot.Middlewares;

public class DiscordUserMiddleware
{
    private readonly RequestDelegate _next;

    public DiscordUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext, IUnitOfWork uow)
    {
        var discordUserIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (discordUserIdClaim != null
            && !string.IsNullOrWhiteSpace(discordUserIdClaim.Value))
        {
            var discordUser = await uow.Users.GetFromIdentityUserId(discordUserIdClaim.Value);
            if (discordUser != null)
            {
                httpContext.Features.Set(discordUser);
            }
        }

        await _next(httpContext);
    }
}