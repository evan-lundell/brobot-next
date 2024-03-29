using AutoMapper;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class ChannelsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMapper _mapper;

    public ChannelsController(
        IUnitOfWork uow,
        UserManager<IdentityUser> userManager,
        IMapper mapper)
    {
        _uow = uow;
        _userManager = userManager;
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ChannelResponse>>> GetChannels()
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        var channels = await _uow.Channels.FindByUser(discordUser.Id);
        return Ok(_mapper.Map<IEnumerable<ChannelResponse>>(channels));
    }
}