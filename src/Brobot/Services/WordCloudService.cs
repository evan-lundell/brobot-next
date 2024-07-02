using Brobot.Repositories;
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
    private readonly IUnitOfWork _uow;
    private readonly ILogger<WordCloudService> _logger;

    public WordCloudService(IUnitOfWork uow, ILogger<WordCloudService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<byte[]> GenerateWordCloud(ulong channelId, int monthsBack = 1)
    {
        try
        {
            var startDateTime = DateTime.UtcNow.AddMonths(-monthsBack);
            var startDate = new DateOnly(startDateTime.Year, startDateTime.Month, 1);
            var endDate = startDate.AddMonths(1);
            _logger.LogInformation("Generating word cloud for channel {ChannelId} from {StartDate} to {EndDate}", channelId, startDate, endDate);
            var wordCounts = (await _uow.WordCounts.GetWordCountsByChannelId(channelId, startDate, endDate)).ToList();
            if (!wordCounts.Any())
            {
                _logger.LogInformation("No word counts found for channel {ChannelId}", channelId);
                return [];
            }

            _logger.LogInformation("Word counts found for channel {ChannelId}: {Count}", channelId, wordCounts.Count);
            var entries = wordCounts.Select(wc => new WordCloudEntry(wc.Word, wc.Count));
            var wordCloud = new WordCloudInput(entries)
            {
                Width = 1024,
                Height = 576,
                MinFontSize = 36,
                MaxFontSize = 96,
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
            _logger.LogInformation("Word cloud generated for channel {ChannelId}, {Bytes} bytes", channelId, data.ToArray().Length);
            return data.ToArray();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error generating word cloud");
            return [];
        }
    }
}