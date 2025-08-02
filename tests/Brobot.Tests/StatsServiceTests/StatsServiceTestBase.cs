using Brobot.Repositories;
using Brobot.Services;
using Discord;
using Moq;

namespace Brobot.Tests.StatsServiceTests;

[TestFixture]
public abstract class StatsServiceTestBase
{
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; private set; }
    protected Mock<IDailyMessageCountRepository> DailyMessageCountRepositoryMock { get; private set; }
    protected Mock<IDiscordClient> DiscordClientMock { get; private set; }
    protected Mock<IWordCountService> WordCountServiceMock { get; private set; }
    protected Mock<IWordCloudService> WordCloudServiceMock { get; private set; }
    protected StatsService StatsService { get; private set; }
    
    [SetUp]
    public void Setup()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        DailyMessageCountRepositoryMock = new Mock<IDailyMessageCountRepository>();
        UnitOfWorkMock.SetupGet(uow => uow.DailyMessageCounts).Returns(DailyMessageCountRepositoryMock.Object);
        DiscordClientMock = new Mock<IDiscordClient>();
        WordCloudServiceMock = new Mock<IWordCloudService>();
        WordCountServiceMock = new Mock<IWordCountService>();
        StatsService = new StatsService(UnitOfWorkMock.Object, DiscordClientMock.Object, WordCountServiceMock.Object, WordCloudServiceMock.Object);
    }
}