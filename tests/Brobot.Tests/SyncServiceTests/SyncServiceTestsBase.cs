using Brobot.Configuration;
using Brobot.Contexts;
using Brobot.Repositories;
using Brobot.Services;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

[TestFixture]
public abstract class SyncServiceTestsBase
{
    protected BrobotDbContext Context;
    protected SyncService SyncService;
    private ServiceProvider _serviceProvider;
    protected Mock<ILogger<SyncService>> LoggerMock;
    protected Mock<IDiscordClient> MockDiscordClient;
    
    [SetUp]
    public virtual void Setup()
    {
        ServiceCollection services = new();
        var uniqueDbName = $"Brobot_{Guid.NewGuid()}";
        services.AddDbContext<BrobotDbContext>(options => options.UseInMemoryDatabase(uniqueDbName));
        services.AddTransient<IUnitOfWork, UnitOfWork>();
        _serviceProvider = services.BuildServiceProvider();
        Context = _serviceProvider.GetRequiredService<BrobotDbContext>();
        SetupDatabase();
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["FixTwitterLinks"]).Returns("true");
        LoggerMock = new Mock<ILogger<SyncService>>();
        MockDiscordClient = new Mock<IDiscordClient>();
        GeneralOptions generalOptions = new()
        {
            FixTwitterLinks = true,
            VersionFilePath = "./version.txt",
            SeqUrl = "http://localhost:5341"
        };
        Mock<IServiceScope> serviceScopeMock = new Mock<IServiceScope>();
        Mock<IServiceScopeFactory> serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(ssf => ssf.CreateScope())
            .Returns(serviceScopeMock.Object);
        serviceScopeMock.SetupGet(ss => ss.ServiceProvider)
            .Returns(_serviceProvider);
        
        SyncService = new SyncService(
            serviceScopeFactoryMock.Object,
            MockDiscordClient.Object,
            LoggerMock.Object,
            Options.Create(generalOptions));
    }

    protected abstract void SetupDatabase();

    [TearDown]
    public virtual void TearDown()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        _serviceProvider.Dispose();
    }
}