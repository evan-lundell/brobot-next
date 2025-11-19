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
public class ScheduledMessagesController(
    IScheduledMessageService scheduledMessageService,
    ILogger<ScheduledMessagesController> logger) : ControllerBase
{
    private const int MaxLimit = 50;
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduledMessageResponse>>> Get(
        [FromQuery] int? limit = null,
        [FromQuery] int skip = 0,
        [FromQuery] DateTime? scheduledBefore = null,
        [FromQuery] DateTime? scheduledAfter = null)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        if (limit > MaxLimit)
        {
            logger.LogWarning("{Limit} exceeds the maximum limit of {MaxLimit}.", limit, MaxLimit);
            return BadRequest($"Limit cannot be greater than {MaxLimit}");
        }
        var scheduledMessages = await scheduledMessageService.GetScheduledMessagesByUser(discordUser, limit, skip, scheduledBefore, scheduledAfter);
        return Ok(scheduledMessages.Select(sm => sm.ToScheduledMessageResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<ScheduledMessageResponse>> CreateScheduledMessage(
        ScheduledMessageRequest scheduledMessage)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        if (scheduledMessage.ChannelId == null)
        {
            logger.LogWarning($"{nameof(scheduledMessage.ChannelId)} is required.");
            return BadRequest("Channel Id not given");
        }

        var newScheduledMessage = await scheduledMessageService.CreateScheduledMessage(scheduledMessage.MessageText,
            discordUser, scheduledMessage.SendDate, scheduledMessage.ChannelId.Value);
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

    [HttpPut("{id}")]
    public async Task<ActionResult<ScheduledMessageResponse>> UpdateScheduledMessage(int id,
        ScheduledMessageRequest scheduledMessageRequest)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        if (!await scheduledMessageService.CanUserUpdateScheduledMessage(discordUser, id))
        {
            logger.LogWarning("User {UserId} is not authorized to update scheduled message {ScheduledMessageId}.", discordUser.Id, id);
            return Unauthorized();
        }

        var updated = await scheduledMessageService.UpdateScheduledMessage(id,
            scheduledMessageRequest.MessageText, scheduledMessageRequest.ChannelId,
            scheduledMessageRequest.SendDate);
        return Ok(updated.ToScheduledMessageResponse());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteScheduledMessage(int id)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        if (!await scheduledMessageService.CanUserUpdateScheduledMessage(discordUser, id))
        {
            logger.LogWarning("User {UserId} is not authorized to delete scheduled message {ScheduledMessageId}.", discordUser.Id, id);
            return Unauthorized();
        }

        if (!await scheduledMessageService.DeleteScheduledMessage(id))
        {
            logger.LogWarning("Scheduled message {ScheduledMessageId} was not found and could not be deleted.", id);
            return NotFound();
        }
        return Ok();
    }
}