using Brobot.Contexts;
using Brobot.Repositories;
using Brobot.Services;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Brobot.Tests.StatsServiceTests;

[TestFixture]
public abstract class StatsServiceTestBase
{
    protected BrobotDbContext Context;
    protected Mock<IDiscordClient> DiscordClientMock { get; private set; }
    protected Mock<IWordCountService> WordCountServiceMock { get; private set; }
    protected Mock<IWordCloudService> WordCloudServiceMock { get; private set; }
    protected StatsService StatsService { get; private set; }
    private ServiceProvider _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        ServiceCollection serviceCollection = new();
        var uniquieDbName = $"Brobot_{Guid.NewGuid()}";
        serviceCollection.AddDbContext<BrobotDbContext>(options => options.UseInMemoryDatabase(uniquieDbName));
        serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
        _serviceProvider = serviceCollection.BuildServiceProvider();
        Context = _serviceProvider.GetRequiredService<BrobotDbContext>();

        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
        DiscordClientMock = new Mock<IDiscordClient>();
        WordCloudServiceMock = new Mock<IWordCloudService>();
        WordCountServiceMock = new Mock<IWordCountService>();
        StatsService = new StatsService(unitOfWork, DiscordClientMock.Object, WordCountServiceMock.Object, WordCloudServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        _serviceProvider.Dispose();
    }
}