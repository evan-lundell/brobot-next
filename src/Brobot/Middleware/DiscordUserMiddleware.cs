using Brobot.Repositories;
using ClaimTypes = Brobot.Shared.Claims.ClaimTypes;

namespace Brobot.Middleware;

public class DiscordUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IUnitOfWork uow)
    {
        // Only process if user is authenticated
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var discordIdClaim = context.User.FindFirst(ClaimTypes.DiscordId)?.Value;
            if (!string.IsNullOrEmpty(discordIdClaim) && ulong.TryParse(discordIdClaim, out var discordId))
            {
                var discordUser = await uow.Users.GetById(discordId);
                if (discordUser != null)
                {
                    context.Features.Set(discordUser);
                }
            }
        }

        await next(context);
    }
}
