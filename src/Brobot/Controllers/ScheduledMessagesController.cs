using Brobot.Exceptions;
using Brobot.Models;
using Brobot.Services;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using TimeZoneConverter;
using Brobot.Mappers;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ScheduledMessagesController(ScheduledMessageService scheduledMessageService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduledMessageResponse>>> Get(
        [FromQuery] int? limit = null,
        [FromQuery] int skip = 0,
        [FromQuery] DateTime? scheduledBefore = null,
        [FromQuery] DateTime? scheduledAfter = null)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        if (limit > 50)
        {
            return BadRequest("Limit cannot be greater than 50");
        }
        var scheduledMessages = await scheduledMessageService.GetScheduledMessagesByUser(discordUser, limit, skip, scheduledBefore, scheduledAfter);
        return Ok(scheduledMessages.Select(sm => sm.ToScheduledMessageResponse()));
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

            var newScheduledMessage = await scheduledMessageService.CreateScheduledMessage(scheduledMessage.MessageText, discordUser, scheduledMessage.SendDate, scheduledMessage.ChannelId.Value);
            // ReSharper disable once InvertIf
            if (newScheduledMessage.SendDate.HasValue &&
                !string.IsNullOrWhiteSpace(newScheduledMessage.CreatedBy.Timezone))
            {
                var timezone = TZConvert.GetTimeZoneInfo(newScheduledMessage.CreatedBy.Timezone);
                var offset = timezone.GetUtcOffset(DateTime.UtcNow);
                newScheduledMessage.SendDate = newScheduledMessage.SendDate.Value.ToOffset(offset);
            }

            return Ok(newScheduledMessage.ToScheduledMessageResponse());
        }
        catch (Exception e)
        {
            return BadRequest(new ErrorResponse { Title = e.Message, Type = "Bad Request" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ScheduledMessageResponse>> UpdateScheduledMessage(int id, ScheduledMessageRequest scheduledMessageRequest)
    {
        try
        {
            var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
            if (!await scheduledMessageService.CanUserUpdateScheduledMessage(discordUser, id))
            {
                return Unauthorized();
            }

            var updated = await scheduledMessageService.UpdateScheduledMessage(id,
                scheduledMessageRequest.MessageText, scheduledMessageRequest.ChannelId,
                scheduledMessageRequest.SendDate);
            return Ok(updated.ToScheduledMessageResponse());
        }
        catch (ModelNotFoundException mnfEx)
        {
            return NotFound(mnfEx.Message);
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
        if (!await scheduledMessageService.CanUserUpdateScheduledMessage(discordUser, id))
        {
            return Unauthorized();
        }

        if (!await scheduledMessageService.DeleteScheduledMessage(id))
        {
            return NotFound();
        }
        return Ok();
    }
}