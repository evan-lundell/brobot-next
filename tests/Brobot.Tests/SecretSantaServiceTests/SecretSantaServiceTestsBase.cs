using Brobot.Contexts;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Brobot.Tests.SecretSantaServiceTests;

[TestFixture]
public abstract class SecretSantaServiceTestsBase
{
    protected BrobotDbContext Context;
    protected SecretSantaService SecretSantaService;
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

        List<SecretSantaGroupUserModel> group1Users = new();
        for (ulong i = 1; i <= 6; i++)
        {
            group1Users.Add(CreateSecretSantaGroupUserModel(i, group1.Entity));
        }
        for (int i = 0; i < group1Users.Count; i++)
        {
            CreateSecretSantaPairModel(group1.Entity, group1Users[i], group1Users[(i + 1) % group1Users.Count], DateTime.UtcNow.AddYears(-1).Year);
        }

        List<SecretSantaGroupUserModel> group2Users = new();
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
        
        var discordClientMock = new Mock<IDiscordClient>();
        
        Random random = new(5000);
        SecretSantaService = new SecretSantaService(unitOfWork, discordClientMock.Object, random);
    }
    
    [TearDown]
    public void TearDown()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        _serviceProvider.Dispose();
    }
    
    private SecretSantaGroupUserModel CreateSecretSantaGroupUserModel(ulong userId, SecretSantaGroupModel secretSantaGroup)
    {
        var groupUser = new SecretSantaGroupUserModel
        {
            UserId = userId,
            SecretSantaGroupId = secretSantaGroup.Id,
            User = new UserModel
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
        SecretSantaGroupUserModel giver,
        SecretSantaGroupUserModel recipient,
        int year)
    {
        var pair = new SecretSantaPairModel
        {
            SecretSantaGroup = group,
            SecretSantaGroupId = group.Id,
            GiverUser = giver.User,
            GiverUserId = giver.User.Id,
            RecipientUser = recipient.User,
            RecipientUserId = recipient.User.Id,
            Year = year
        };
        group.SecretSantaPairs.Add(pair);
        Context.SecretSantaPairs.Add(pair);
    }
}