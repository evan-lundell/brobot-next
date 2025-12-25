using Brobot.Contexts;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Brobot.Tests.SecretSantaServiceTests;

[TestFixture]
public abstract class SecretSantaServiceTestsBase
{
    protected BrobotDbContext Context;
    protected SecretSantaService SecretSantaService;
    protected Mock<IDiscordClient> DiscordClientMock;
    private ServiceProvider _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        ServiceCollection serviceCollection = new();
        var uniqueDbName = $"Brobot_{Guid.NewGuid()}";
        serviceCollection.AddDbContext<BrobotDbContext>(options => options.UseInMemoryDatabase(uniqueDbName));
        _serviceProvider = serviceCollection.BuildServiceProvider();
        Context = _serviceProvider.GetRequiredService<BrobotDbContext>();
        
        var group1 = Context.SecretSantaGroups.Add(new SecretSantaGroupModel
        {
            Id = 1,
            Name = "Test Group 1"
        });

        List<SecretSantaGroupDiscordUserModel> group1Users = new();
        for (ulong i = 1; i <= 6; i++)
        {
            group1Users.Add(CreateSecretSantaGroupUserModel(i, group1.Entity));
        }
        for (int i = 0; i < group1Users.Count; i++)
        {
            CreateSecretSantaPairModel(group1.Entity, group1Users[i], group1Users[(i + 1) % group1Users.Count], DateTime.UtcNow.AddYears(-1).Year);
        }

        List<SecretSantaGroupDiscordUserModel> group2Users = new();
        var group2 = Context.SecretSantaGroups.Add(new SecretSantaGroupModel
        {
            Id = 2,
            Name = "Test Group 2"
        });
        for (ulong i = 1; i <= 6; i++)
        {
            group2Users.Add(CreateSecretSantaGroupUserModel(i + 6, group2.Entity));
        }
        for (int i = 0; i < group2Users.Count; i++)
        {
            CreateSecretSantaPairModel(group2.Entity, group2Users[i], group2Users[(i + 1) % group2Users.Count], DateTime.UtcNow.Year);
        }

        Context.SaveChanges();
        
        var unitOfWork = new UnitOfWork(Context);
        
        DiscordClientMock = new Mock<IDiscordClient>();
        
        Random random = new(5000);
        SecretSantaService = new SecretSantaService(unitOfWork, DiscordClientMock.Object, random, Mock.Of<ILogger<SecretSantaService>>());
    }
    
    [TearDown]
    public void TearDown()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        _serviceProvider.Dispose();
    }
    
    private SecretSantaGroupDiscordUserModel CreateSecretSantaGroupUserModel(ulong userId, SecretSantaGroupModel secretSantaGroup)
    {
        var groupUser = new SecretSantaGroupDiscordUserModel
        {
            DiscordUserId = userId,
            SecretSantaGroupId = secretSantaGroup.Id,
            DiscordUser = new DiscordUserModel
            {
                Id = userId,
                Username = $"User {userId}"
            },
            SecretSantaGroup = secretSantaGroup
        };
        secretSantaGroup.SecretSantaGroupUsers.Add(groupUser);
        return groupUser;
    }

    private void CreateSecretSantaPairModel(
        SecretSantaGroupModel group,
        SecretSantaGroupDiscordUserModel giver,
        SecretSantaGroupDiscordUserModel recipient,
        int year)
    {
        var pair = new SecretSantaPairModel
        {
            SecretSantaGroup = group,
            SecretSantaGroupId = group.Id,
            GiverDiscordUser = giver.DiscordUser,
            GiverDiscordUserId = giver.DiscordUser.Id,
            RecipientDiscordUser = recipient.DiscordUser,
            RecipientDiscordUserId = recipient.DiscordUser.Id,
            Year = year
        };
        group.SecretSantaPairs.Add(pair);
        Context.SecretSantaPairs.Add(pair);
    }
}