using Brobot.Contexts;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Brobot.Tests.HotOpServiceTests;

[TestFixture]
public abstract class HotOpServiceTestsBase
{
    protected BrobotDbContext Context;
    protected HotOpService HotOpService;
    
    [SetUp]
    public void Setup()
    {
        ServiceCollection serviceCollection = new();
        serviceCollection.AddDbContext<BrobotDbContext>(options => options.UseInMemoryDatabase("Brobot"));
        serviceCollection.AddTransient<IUnitOfWork, UnitOfWork>();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        Context = serviceProvider.GetRequiredService<BrobotDbContext>();

        var hotOpModel = new HotOpModel
        {
            Id = 1,
            Channel = new ChannelModel
            {
                Id = 1,
                Name = "Test Channel",
                GuildId = 1,
                Guild = new GuildModel
                {
                    Id = 1,
                    Name = "Test Guild"
                }
            },
            ChannelId = 1,
            StartDate = DateTimeOffset.UtcNow.AddDays(-1),
            EndDate = DateTimeOffset.UtcNow.AddDays(1),
            User = new UserModel
            {
                Id = 1,
                Username = "Test User"
            },
            UserId = 1
        };
        Context.HotOps.Add(hotOpModel);
        
        CreateHotOpSession(hotOpModel, hotOpModel.User, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));
        
        HotOpService = new HotOpService(serviceProvider);
    }
    
    [TearDown]
    public void TearDown()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }

    private void CreateHotOpSession(HotOpModel hotOpModel, UserModel user, DateTimeOffset startDateTime, DateTimeOffset endDateTime)
    {
        var hotOpSession = new HotOpSessionModel
        {
            HotOp = hotOpModel,
            HotOpId = hotOpModel.Id,
            User = user,
            UserId = user.Id,
            StartDateTime = DateTimeOffset.UtcNow
        };
        
        hotOpModel.HotOpSessions.Add(hotOpSession);
    }
}