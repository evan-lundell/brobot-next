using Brobot.Models;
using Discord;
using Discord.WebSocket;
using TimeZoneConverter;

namespace Brobot.Services;

public class WordCountService
{
    private readonly ILogger<WordCountService> _logger;
    private readonly DiscordSocketClient _client;
    private readonly string[] _separators =
        [" ", "\t", "\n", "\r\n", ",", ":", ".", "!", "/", "\\", "%", "&", "?", "(", ")", "\"", "@"];
    
    public WordCountService(ILogger<WordCountService> logger, DiscordSocketClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<IEnumerable<WordCountModel>> GetWordCount(ChannelModel channel, int daysBack = 1)
    {
        try
        {
            var socketChannel = await _client.GetChannelAsync(channel.Id);
            if (socketChannel is not SocketTextChannel socketTextChannel)
            {
                return [];
            }

            var timezone = TZConvert.GetTimeZoneInfo(channel.Timezone);
            var utcNow = DateTime.UtcNow;
            var localDay = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timezone).AddDays(-daysBack);
            var day = new DateTimeOffset(localDay.Year, localDay.Month, localDay.Day, 0, 0, 0, timezone.GetUtcOffset(DateTime.UtcNow));
            var nextDay = day.AddDays(1);
            var start = new DateTimeOffset(day.Year, day.Month, day.Day, 0, 0, 0, day.Offset);
            var end = new DateTimeOffset(nextDay.Year, nextDay.Month, nextDay.Day, 0, 0, 0, nextDay.Offset);
            var doneWithDay = false;
            ulong? fromMessageId = null;
            Dictionary<string, WordCountModel> wordCounts = new();

            while (!doneWithDay)
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
                        doneWithDay = true;
                        break;
                    }

                    var wordSplit = message.Content.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in wordSplit)
                    {
                        wordCounts.TryAdd(word.ToLower(), new WordCountModel
                        {
                            Word = word.ToLower(),
                            Count = 0,
                            Channel = channel,
                            ChannelId = channel.Id,
                            CountDate = new DateOnly(start.Year, start.Month, start.Day)
                        });
                        wordCounts[word.ToLower()].Count += 1;
                    }
                }

                if (!doneWithDay)
                {
                    await Task.Delay(2500);
                }
            }

            return wordCounts.Values;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Word Count failed");
            return [];
        }
    }
}