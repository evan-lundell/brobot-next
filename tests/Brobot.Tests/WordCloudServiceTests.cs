using Brobot.Contexts;
using Brobot.Models;
using Brobot.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Brobot.Tests;

[TestFixture]
public class WordCloudServiceTests
{
    private IUnitOfWork _unitOfWork;
    private ServiceProvider _serviceProvider;
    private BrobotDbContext _context;
    
    [SetUp]
    public void Setup()
    {
        ServiceCollection serviceCollection = new();
        serviceCollection.AddDbContext<BrobotDbContext>(options => options.UseLazyLoadingProxies().UseInMemoryDatabase("Brobot"));
        _serviceProvider = serviceCollection.BuildServiceProvider();

        _context = _serviceProvider.GetRequiredService<BrobotDbContext>();
        var guildEntry = _context.Guilds.Add(new GuildModel
        {
            Id = 1,
            Name = "Test Guild",
            Archived = false
        });
        _context.Channels.Add(new ChannelModel
        {
            Id = 1,
            Name = "Test Channel 1",
            Guild = guildEntry.Entity,
            GuildId = 1
        });
        _context.Channels.Add(new ChannelModel
        {
            Id = 2,
            Name = "Test Channel 2",
            Guild = guildEntry.Entity,
            GuildId = 1
        });
        
        _unitOfWork = new UnitOfWork(_context);
    }

    [Test]
    public async Task GenerateWordCloud_NoWordCount_ReturnsEmptyArray()
    {
        
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _serviceProvider.Dispose();
    }
}