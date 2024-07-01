using Brobot.Repositories;
using Brobot.Services;
using Brobot.Shared.Requests;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class MessagesController : ControllerBase
{
    private readonly DiscordSocketClient _client;

    public MessagesController(DiscordSocketClient client)
    {
        _client = client;
    }
    
    [HttpPost("send")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendMessage(SendMessageRequest sendMessageRequest)
    {
        if (await _client.GetChannelAsync(sendMessageRequest.ChannelId) is not SocketTextChannel channel)
        {
            return BadRequest("Cannot find channel");
        }

        await channel.SendMessageAsync(sendMessageRequest.Message);
        return Ok();
    }
}