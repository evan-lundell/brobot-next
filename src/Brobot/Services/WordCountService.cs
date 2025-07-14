using Brobot.Dtos;
using Brobot.Models;
using Discord;
using Discord.WebSocket;
using TimeZoneConverter;

namespace Brobot.Services;

public class WordCountService(ILogger<WordCountService> logger, IDiscordClient client, IStopWordService stopWordService)
{
    private readonly string[] _separators =
        [" ", "\t", "\n", "\r\n", ",", ":", ".", "!", "/", "\\", "%", "&", "?", "\"", "@", "*", "<", ">", "[", "]", "(", ")", "-", ";", "*", "{", "}", "+", "=", "#", "~"];

    public async Task<IEnumerable<WordCountDto>> GetWordCount(ChannelModel channel, DateTime startDate, DateTime endDate)
    {
        try
        {
            var socketChannel = await client.GetChannelAsync(channel.Id);
            if (socketChannel is not ISocketMessageChannel socketTextChannel)
            {
                return [];
            }

            var timezone = TZConvert.GetTimeZoneInfo(channel.Timezone);
            var start = new DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(startDate, timezone), timezone.GetUtcOffset(startDate));
            var end = new DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(endDate, timezone), timezone.GetUtcOffset(endDate));
            ulong? fromMessageId = null;
            var done = false;
            Dictionary<string, WordCountDto> wordCounts = new();

            while (!done)
            {
                IAsyncEnumerable<IReadOnlyCollection<IMessage>> messageCollection = fromMessageId.HasValue
                    ? socketTextChannel.GetMessagesAsync(fromMessageId.Value, Direction.Before)
                    : socketTextChannel.GetMessagesAsync();
                var messages = (await messageCollection.FlattenAsync()).ToList();
                if (messages.Count == 0)
                {
                    break;
                }
                foreach (var message in messages.Where(message => message.Timestamp < end))
                {
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
                        var wordLower = word.ToLowerInvariant();
                        wordCounts.TryAdd(wordLower, new WordCountDto
                        {
                            Word = wordLower,
                            Count = 0,
                            ChannelId = channel.Id
                        });
                        wordCounts[wordLower].Count += 1;
                    }

                    fromMessageId = message.Id;
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