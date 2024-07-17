using Brobot.Repositories;
using Brobot.Services;
using Brobot.Shared.Requests;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class WordCloudController : ControllerBase
{
    private readonly WordCloudService _wordCloudService;
    private readonly DiscordSocketClient _client;
    private readonly IUnitOfWork _uow;

    public WordCloudController(WordCloudService wordCloudService, DiscordSocketClient client, IUnitOfWork uow)
    {
        _wordCloudService = wordCloudService;
        _client = client;
        _uow = uow;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendWordCloud(WordCloudRequest wordCloudRequest)
    {
        try
        {
            var channel = await _uow.Channels.GetById(wordCloudRequest.ChannelId);
            if (channel == null)
            {
                return BadRequest("Channel not found");
            }
            var socketTextChannel = await _client.GetChannelAsync(wordCloudRequest.ChannelId) as SocketTextChannel;
            if (socketTextChannel == null)
            {
                return BadRequest("Channel not found");
            }
            var wordCloud = await _wordCloudService.GenerateWordCloud(wordCloudRequest.ChannelId, wordCloudRequest.MonthsBack);
            var fileName = "wordcloud.png";
            await System.IO.File.WriteAllBytesAsync(fileName, wordCloud);
            await socketTextChannel.SendFileAsync(fileName);
            System.IO.File.Delete(fileName);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}