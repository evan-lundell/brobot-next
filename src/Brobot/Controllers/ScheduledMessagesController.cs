using AutoMapper;
using Brobot.Models;
using Brobot.Services;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using TimeZoneConverter;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ScheduledMessagesController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ScheduledMessageService _scheduledMessageService;

    public ScheduledMessagesController(IMapper mapper, ScheduledMessageService scheduledMessageService)
    {
        _mapper = mapper;
        _scheduledMessageService = scheduledMessageService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduledMessageResponse>>> Get(
        [FromQuery] int limit = 10,
        [FromQuery] int skip = 0,
        [FromQuery] DateTime? scheduledBefore = null,
        [FromQuery] DateTime? scheduledAfter = null)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        if (limit > 50)
        {
            return BadRequest("Limit cannot be greater than 50");
        }
        
        return Ok(_mapper.Map<IEnumerable<ScheduledMessageResponse>>(await _scheduledMessageService.GetScheduledMessagesByUser(discordUser, limit, skip, scheduledBefore, scheduledAfter)));
    }

    [HttpPost]
    public async Task<ActionResult<ScheduledMessageResponse>> CreateScheduledMessage(ScheduledMessageRequest scheduledMessage)
    {
        try
        {
            var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
            if (scheduledMessage.ChannelId == null)
            {
                return BadRequest("Channel Id not given");
            }

            var newScheduledMessage = await _scheduledMessageService.CreateScheduledMessage(scheduledMessage.MessageText, discordUser, scheduledMessage.SendDate, scheduledMessage.ChannelId.Value);
            // ReSharper disable once InvertIf
            if (newScheduledMessage.SendDate.HasValue &&
                !string.IsNullOrWhiteSpace(newScheduledMessage.CreatedBy.Timezone))
            {
                var timezone = TZConvert.GetTimeZoneInfo(newScheduledMessage.CreatedBy.Timezone);
                var offset = timezone.GetUtcOffset(DateTime.UtcNow);
                newScheduledMessage.SendDate = newScheduledMessage.SendDate.Value.ToOffset(offset);
            }

            return Ok(_mapper.Map<ScheduledMessageResponse>(newScheduledMessage));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ScheduledMessageResponse>> UpdateScheduledMessage(int id, ScheduledMessageRequest scheduledMessageRequest)
    {
        try
        {
            var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
            if (!await _scheduledMessageService.CanUserUpdateScheduledMessage(discordUser, id))
            {
                return Unauthorized();
            }

            return Ok(_mapper.Map<ScheduledMessageResponse>(await _scheduledMessageService.UpdateScheduledMessage(id,
                scheduledMessageRequest.MessageText, scheduledMessageRequest.ChannelId,
                scheduledMessageRequest.SendDate)));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteScheduledMessage(int id)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        if (!await _scheduledMessageService.CanUserUpdateScheduledMessage(discordUser, id))
        {
            return Unauthorized();
        }

        if (!await _scheduledMessageService.DeleteScheduledMessage(id))
        {
            return NotFound();
        }
        return Ok();
    }
}