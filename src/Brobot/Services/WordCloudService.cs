using Brobot.Models;
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

    private const string WordcloudPath = "wordcloud.png";

    public WordCloudService(
        DiscordSocketClient client,
        StopWordService stopWordService,
        ILogger<WordCloudService> logger)
    {
        _client = client;
        _stopWordService = stopWordService;
        _logger = logger;
    }

    public async Task GenerateWordCloud(ChannelModel channel)
    {
        try
        {
            var wordCount = new Dictionary<string, int>();
            var socketChannel = await _client.GetChannelAsync(channel.Id);
            if (socketChannel is not SocketTextChannel socketTextChannel)
            {
                return;
            }

            ulong? fromMessageId = null;
            string[] separators =
                { " ", "\t", "\n", "\r\n", ",", ":", ".", "!", "/", "\\", "%", "&", "?", "(", ")", "\"", "@" };

            bool doneWithMonth = false;
            // TODO: Uncomment this line and get ride of the following
            // int lastMonth = DateTime.UtcNow.Month - 1;
            int lastMonth = DateTime.UtcNow.Month;
            while (!doneWithMonth)
            {
                IAsyncEnumerable<IReadOnlyCollection<IMessage>> previousMessages = fromMessageId.HasValue
                    ? socketTextChannel.GetMessagesAsync(fromMessageId.Value, Direction.Before)
                    : socketTextChannel.GetMessagesAsync();

                var messages = await previousMessages.FlattenAsync();
                foreach (var message in messages)
                {
                    if (message.Timestamp.Month != lastMonth)
                    {
                        doneWithMonth = true;
                        break;
                    }

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
                return;
            }

            var frequencies = wordCount
                .OrderByDescending((w) => w.Value)
                .Take(100)
                .Select((w) => new WordCloudEntry(w.Key, w.Value));
           GenerateFile(frequencies);
            await socketTextChannel.SendFileAsync(WordcloudPath, string.Empty);
            File.Delete(WordcloudPath);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to generate word cloud for {ChannelId}", channel.Id);
        }
    }

    private void GenerateFile(IEnumerable<WordCloudEntry> entries)
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
        using var writer = File.Create(WordcloudPath);
        data.SaveTo(writer);
    }
}