using Brobot.Models;
using Brobot.Repositories;
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
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ChannelResponse>>> GetChannels()
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        var channels = await uow.Channels.FindByUser(discordUser.Id);
        return Ok(channels.Select(c => c.ToChannelResponse()));
    }
}