using Brobot.Configuration;
using Brobot.Contexts;
using Brobot.Repositories;
using Brobot.Services;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Brobot.Tests.VersionServiceTests;

[TestFixture]
public abstract class VersionServiceTestsBase
{
    protected Mock<IDiscordClient> MockClient;
    protected Mock<ILogger<VersionService>> MockLogger;
    protected Mock<IOptions<GeneralOptions>> MockGeneralOptions;
    protected VersionService VersionService;
    protected Mock<IAssemblyService> MockAssemblyService;
    protected BrobotDbContext Context;
    
    private ServiceProvider _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        MockClient = new Mock<IDiscordClient>();
        MockLogger = new Mock<ILogger<VersionService>>();
        MockGeneralOptions = new Mock<IOptions<GeneralOptions>>();
        MockAssemblyService = new Mock<IAssemblyService>();

        GeneralOptions generalOptions = new()
        {
            VersionFilePath = "",
            ReleaseNotesUrl = "https://example.com/releases"
        };
        MockGeneralOptions.Setup(x => x.Value).Returns(generalOptions);

        ServiceCollection serviceCollection = new();
        var uniqueName = $"Brobot_{Guid.NewGuid()}";
        serviceCollection.AddDbContext<BrobotDbContext>(options => options.UseInMemoryDatabase(uniqueName));
        serviceCollection.AddTransient<IUnitOfWork, UnitOfWork>();
        _serviceProvider = serviceCollection.BuildServiceProvider();
        Context = _serviceProvider.GetRequiredService<BrobotDbContext>();
        
        VersionService = new VersionService(
            _serviceProvider.GetRequiredService<IUnitOfWork>(),
            MockClient.Object,
            MockLogger.Object,
            MockGeneralOptions.Object,
            MockAssemblyService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        _serviceProvider.Dispose();
    }
}