using AutoMapper;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ScheduledMessageController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ScheduledMessageController(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduledMessageResponse>>> Get(
        [FromQuery] int limit = 10,
        [FromQuery] int skip = 0,
        [FromQuery] DateTime? scheduledBefore = null,
        [FromQuery] DateTime? scheduledAfter = null)
    {
        if (!(HttpContext.Items["DiscordUser"] is UserModel discordUser))
        {
            return Unauthorized();
        }

        if (limit > 50)
        {
            return BadRequest("Limit cannont be greater than 50");
        }

        var scheduledMessages = await _uow.ScheduledMessages.GetScheduledMessagesByUser(discordUser.Id, limit, skip, scheduledBefore, scheduledAfter);
        return Ok(_mapper.Map<IEnumerable<ScheduledMessageResponse>>(scheduledMessages));
    }
}