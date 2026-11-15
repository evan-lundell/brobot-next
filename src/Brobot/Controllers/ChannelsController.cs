using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Brobot.Mappers;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class ChannelsController(IUnitOfWork uow) : ControllerBase
{
    /// <summary>
    /// Gets the channels the caller is a member of. Admins can pass <paramref name="userId"/> to get another
    /// user's channels instead, e.g. to pick a channel for a hot op they're creating for that user.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ChannelResponse>>> GetChannels(
        [FromQuery] ulong? userId = null,
        CancellationToken cancellationToken = default)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<DiscordUserModel>();
        if (userId is not null && userId != discordUser.Id && !User.IsInRole(Constants.AdminRoleName))
        {
            return Forbid();
        }

        var channels = await uow.Channels.FindByUser(userId ?? discordUser.Id, cancellationToken);
        return Ok(channels.Select(c => c.ToChannelResponse()));
    }
}
