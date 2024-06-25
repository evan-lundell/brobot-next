using AutoMapper;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class ChannelsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ChannelsController(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ChannelResponse>>> GetChannels()
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        var channels = await _uow.Channels.FindByUser(discordUser.Id);
        return Ok(_mapper.Map<IEnumerable<ChannelResponse>>(channels));
    }
}