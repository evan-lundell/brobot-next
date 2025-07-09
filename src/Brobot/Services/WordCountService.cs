using Brobot.Dtos;
using Brobot.Models;
using Discord;
using Discord.WebSocket;
using TimeZoneConverter;

namespace Brobot.Services;

public class WordCountService(ILogger<WordCountService> logger, DiscordSocketClient client, StopWordService stopWordService)
{
    private readonly string[] _separators =
        [" ", "\t", "\n", "\r\n", ",", ":", ".", "!", "/", "\\", "%", "&", "?", "\"", "@", "*", "<", ">", "[", "]", "(", ")", "-", ";", "*", "{", "}", "+", "=", "#", "~"];

    public async Task<IEnumerable<WordCountDto>> GetWordCount(ChannelModel channel, DateTime startDate, DateTime endDate)
    {
        try
        {
            var socketChannel = await client.GetChannelAsync(channel.Id);
            if (socketChannel is not SocketTextChannel socketTextChannel)
            {
                return [];
            }

            var timezone = TZConvert.GetTimeZoneInfo(channel.Timezone);
            var start = new DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(startDate, timezone));
            var end = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(endDate, timezone));
            ulong? fromMessageId = null;
            var done = false;
            Dictionary<string, WordCountDto> wordCounts = new();

            while (!done)
            {
                IAsyncEnumerable<IReadOnlyCollection<IMessage>> messageCollection = fromMessageId.HasValue
                    ? socketTextChannel.GetMessagesAsync(fromMessageId.Value, Direction.Before)
                    : socketTextChannel.GetMessagesAsync();
                var messages = await messageCollection.FlattenAsync(); 
                foreach (var message in messages)
                {
                    if (message.Timestamp >= end)
                    {
                        continue;
                    }

                    if (message.Timestamp < start)
                    {
                        done = true;
                        break;
                    }

                    var wordSplit = message.Content.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in wordSplit)
                    {
                        if (await stopWordService.IsStopWord(word))
                        {
                            continue;
                        }
                        wordCounts.TryAdd(word.ToLower(), new WordCountDto
                        {
                            Word = word.ToLower(),
                            Count = 0,
                            ChannelId = channel.Id
                        });
                        wordCounts[word.ToLower()].Count += 1;
                    }
                }

                if (!done)
                {
                    await Task.Delay(2500);
                }
            }

            return wordCounts.Values;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Word Count failed");
            return [];
        }
    }
}