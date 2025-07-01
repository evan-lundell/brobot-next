using Brobot.Contexts;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brobot.Tests;

public class WordCloudServiceTests
{
    private BrobotDbContext _context;
    private IUnitOfWork _uow;
    private ILogger<WordCloudService> _logger;
    
    [SetUp]
    public void Setup()
    {
        ServiceCollection serviceCollection = new();
        serviceCollection.AddDbContext<BrobotDbContext>(options =>
            options.UseInMemoryDatabase("Brobot"));
        serviceCollection.AddTransient<IUnitOfWork, UnitOfWork>();
        serviceCollection.AddLogging(configure => configure.AddConsole());
        var serviceProvider = serviceCollection.BuildServiceProvider();
        _context = serviceProvider.GetRequiredService<BrobotDbContext>();
        _uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        _logger = serviceProvider.GetRequiredService<ILogger<WordCloudService>>();
    }
    
    [Test]
    public async Task GenerateWordCloud_ReturnsByteArray()
    {
        // Arrange
        var channelId = 1234567890UL;
        var monthsBack = 1;

        GuildModel guild = new() { Id = 1, Name = "test-guild" };
        _context.Guilds.Add(guild);
        ChannelModel channel = new() { Id = channelId, Guild = guild, GuildId = guild.Id, Name = "test-channel" };
        _context.Channels.Add(channel);

        var date = DateOnly.FromDateTime(DateTime.Now);
        if (date.Day == 1)
        {
            date = date.AddDays(-1);
        }
        
        // Add test data to the database
        _context.WordCounts.AddRange(
            new WordCountModel { ChannelId = channelId, Channel = channel, Word = "test", Count = 5, CountDate = date },
            new WordCountModel { ChannelId = channelId, Channel = channel, Word = "word", Count = 10, CountDate = date }
        );
        await _context.SaveChangesAsync();

        var wordCloudService = new WordCloudService(_uow, _logger);

        // Act
        var result = await wordCloudService.GenerateWordCloud(channelId, monthsBack);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _uow.Dispose();
    }
}