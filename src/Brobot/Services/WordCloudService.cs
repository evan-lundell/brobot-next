using Brobot.Models;
using Brobot.Shared.Responses;
using Discord;
using Discord.WebSocket;
using KnowledgePicker.WordCloud;
using KnowledgePicker.WordCloud.Coloring;
using KnowledgePicker.WordCloud.Drawing;
using KnowledgePicker.WordCloud.Layouts;
using KnowledgePicker.WordCloud.Primitives;
using KnowledgePicker.WordCloud.Sizers;
using SkiaSharp;

namespace Brobot.Services;

public class WordCloudService
{
    private readonly DiscordSocketClient _client;
    private readonly StopWordService _stopWordService;
    private readonly ILogger<WordCloudService> _logger;

    public WordCloudService(
        DiscordSocketClient client,
        StopWordService stopWordService,
        ILogger<WordCloudService> logger)
    {
        _client = client;
        _stopWordService = stopWordService;
        _logger = logger;
    }

    public async Task<MonthlyWordCloudResponse> GenerateWordCloud(ChannelModel channel, int monthsBack = 1)
    {
        try
        {
            Dictionary<string, int> wordCount = new();
            var socketChannel = await _client.GetChannelAsync(channel.Id);
            if (socketChannel is not SocketTextChannel socketTextChannel)
            {
                return new MonthlyWordCloudResponse
                {
                    Image = [],
                    UserMessageCounts = new Dictionary<string, int>()
                };
            }

            ulong? fromMessageId = null;
            string[] separators =
                [" ", "\t", "\n", "\r\n", ",", ":", ".", "!", "/", "\\", "%", "&", "?", "(", ")", "\"", "@"];

            bool doneWithMonth = false;
            var targetMonth = DateTime.UtcNow.AddMonths(monthsBack * -1);
            var startDateTime = new DateTimeOffset(targetMonth.Year, targetMonth.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var endDateTime = startDateTime.AddMonths(1);
            Dictionary<string, int> userMessageCount = new();
            while (!doneWithMonth)
            {
                IAsyncEnumerable<IReadOnlyCollection<IMessage>> previousMessages = fromMessageId.HasValue
                    ? socketTextChannel.GetMessagesAsync(fromMessageId.Value, Direction.Before)
                    : socketTextChannel.GetMessagesAsync();

                var messages = await previousMessages.FlattenAsync();
                foreach (var message in messages)
                {
                    if (message.Timestamp > endDateTime)
                    {
                        continue;
                    }
                    if (message.Timestamp < startDateTime)
                    {
                        doneWithMonth = true;
                        break;
                    }

                    userMessageCount.TryAdd(message.Author.Username, 0);
                    userMessageCount[message.Author.Username] += 1;
                    var wordSplit = message.Content.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in wordSplit)
                    {
                        if (await _stopWordService.IsStopWord(word.ToLower()))
                        {
                            continue;
                        }

                        wordCount.TryAdd(word.ToLower(), 0);
                        wordCount[word.ToLower()] += 1;
                    }
                }
            }

            if (wordCount.Count == 0)
            {
                return new MonthlyWordCloudResponse
                {
                    Image = [],
                    UserMessageCounts = new Dictionary<string, int>()
                };;
            }

            var frequencies = wordCount
                .OrderByDescending(w => w.Value)
                .Take(100)
                .Select(w => new WordCloudEntry(w.Key, w.Value));
           var image = GenerateFile(frequencies);
           return new MonthlyWordCloudResponse
           {
               Image = image,
               UserMessageCounts = userMessageCount
           };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to generate word cloud for {ChannelId}", channel.Id);
            return new MonthlyWordCloudResponse
            {
                Image = [],
                UserMessageCounts = new Dictionary<string, int>()
            };
        }
    }

    private byte[] GenerateFile(IEnumerable<WordCloudEntry> entries)
    {
        var wordCloud = new WordCloudInput(entries)
        {
            Width = 1024,
            Height = 576,
            MinFontSize = 36,
            MaxFontSize = 96
        };
            
        var sizer = new LogSizer(wordCloud);
        using var engine = new SkGraphicEngine(sizer, wordCloud);
        var layout = new SpiralLayout(wordCloud);
        var colorizer = new RandomColorizer();
        var wcg = new WordCloudGenerator<SKBitmap>(wordCloud, engine, layout, colorizer);
        using var final = new SKBitmap(wordCloud.Width, wordCloud.Height);
        using var canvas = new SKCanvas(final);
        canvas.Clear(SKColors.White);
        using var bitmap = wcg.Draw();
        canvas.DrawBitmap(bitmap, 0, 0);
        using var data = final.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}