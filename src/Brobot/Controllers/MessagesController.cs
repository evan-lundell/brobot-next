using Brobot.Shared.Requests;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class MessagesController(IDiscordClient client, ILogger<MessagesController> logger) : ControllerBase
{
    [HttpPost("send")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendMessage(SendMessageRequest sendMessageRequest)
    {
        if (await client.GetChannelAsync(sendMessageRequest.ChannelId) is not ISocketMessageChannel channel)
        {
            logger.LogWarning("Channel {ChannelId} not found", sendMessageRequest.ChannelId);
            return BadRequest("Cannot find channel");
        }

        await channel.SendMessageAsync(sendMessageRequest.Message);
        return Ok();
    }
}