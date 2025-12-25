using Brobot.Contexts;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Brobot.Tests.ScheduledMessageServiceTests;

[TestFixture]
public abstract class ScheduledMessageServiceTestBase
{
    protected BrobotDbContext Context;
    protected ScheduledMessageService ScheduledMessageService;
    private ServiceProvider _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        ServiceCollection serviceCollection = new();
        var uniqueDbName = $"Brobot_{Guid.NewGuid()}";
        serviceCollection.AddDbContext<BrobotDbContext>(options => options.UseInMemoryDatabase(uniqueDbName));
        serviceCollection.AddTransient<IUnitOfWork, UnitOfWork>();
        _serviceProvider = serviceCollection.BuildServiceProvider();
        Context = _serviceProvider.GetRequiredService<BrobotDbContext>();
        SetupDatabase();
        ScheduledMessageService = new ScheduledMessageService(_serviceProvider.GetRequiredService<IUnitOfWork>(), Mock.Of<ILogger<ScheduledMessageService>>());
    }
    
    [TearDown]
    public void TearDown()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        _serviceProvider.Dispose();
    }

    protected virtual void SetupDatabase()
    {
        var guild = new GuildModel
        {
            Id = 1,
            Name = "Test Guild"
        };
        Context.Guilds.Add(guild);
        
        ChannelModel[] channels =
        [
            new ChannelModel
            {
                Id = 1,
                Name = "Test Channel 1",
                GuildId = 1,
                Guild = guild
            },
            new ChannelModel
            {
                Id = 2,
                Name = "Test Channel 2",
                GuildId = 1,
                Guild = guild
            }
        ];
        Context.Channels.AddRange(channels);
        
        DiscordUserModel[] users =
        [
            CreateUser(1, guild, channels, "america/chicago"),
            CreateUser(2, guild, channels),
            CreateUser(3, guild, [channels[0]], "america/chicago"),
            CreateUser(4, guild, [channels[1]])
        ];
        
        CreateScheduledMessage("Upcoming Test Message 1", users[0], channels[0], DateTimeOffset.UtcNow.AddHours(1));
        CreateScheduledMessage("Upcoming Test Message 2", users[1], channels[0], DateTimeOffset.UtcNow.AddHours(2));
        CreateScheduledMessage("Upcoming Test Message 3", users[0], channels[1], DateTimeOffset.UtcNow.AddHours(2));
        CreateScheduledMessage("Past Test Message 1", users[2], channels[0], DateTimeOffset.UtcNow.AddHours(-3), DateTimeOffset.UtcNow.AddHours(-3));
        CreateScheduledMessage("Past Test Message 2", users[1], channels[1], DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-2));
        
        Context.SaveChanges();
    }

    protected DiscordUserModel CreateUser(ulong id, GuildModel guild, ChannelModel[] channels, string? timezone = null)
    {
        var user = new DiscordUserModel
        {
            Id = id,
            Username = $"Test User {id}",
            Timezone = timezone
        };
        Context.DiscordUsers.Add(user);
        var guildUser = new GuildDiscordUserModel
        {
            DiscordUserId = user.Id,
            GuildId = guild.Id,
            Guild = guild,
            DiscordUser = user
        };
        guild.GuildDiscordUsers.Add(guildUser);
        user.GuildUsers.Add(guildUser);
        
        foreach (var channel in channels)
        {
            var userChannel = new ChannelDiscordUserModel
            {
                UserId = user.Id,
                ChannelId = channel.Id,
                Channel = channel,
                DiscordUser = user
            };
            user.ChannelUsers.Add(userChannel);
            channel.ChannelUsers.Add(userChannel);
        }

        return user;
    }

    protected void CreateScheduledMessage(string messageText, DiscordUserModel createdBy, ChannelModel channel, DateTimeOffset sendDate, DateTimeOffset? sentDate = null)
    {
        var scheduledMessage = new ScheduledMessageModel
        {
            MessageText = messageText,
            SendDate = sendDate,
            SentDate = sentDate,
            ChannelId = channel.Id,
            Channel = channel,
            CreatedBy = createdBy,
            CreatedById = createdBy.Id
        };
        Context.ScheduledMessages.Add(scheduledMessage);
        createdBy.ScheduledMessages.Add(scheduledMessage);
        channel.ScheduledMessages.Add(scheduledMessage);
    }
}