using Brobot.Models;
using Brobot.Repositories;
using Discord;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class ReminderTests : BrobotModuleTestBase
{
    [Test]
    public async Task InvalidDateFormat_RespondsWithInvalidDateFormatMessage()
    {
        await BrobotModule.Reminder("invalid date", "Test reminder");

        AssertRespondAsyncCalledOnce("Invalid date format. Please use yyyy-MM-dd HH:mm");
    }

    [Test]
    public async Task UserNotFound_RespondsWithUserNotFoundMessage()
    {
        const ulong userId = 1;
        Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
        UnitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
        var dateString = DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:mm");
        Mock<IUser> userMock = new();
        userMock.SetupGet(x => x.Id).Returns(userId);
        InteractionContextMock.SetupGet(x => x.User).Returns(userMock.Object);
        
        await BrobotModule.Reminder(dateString, "Test reminder");
        
        AssertRespondAsyncCalledOnce("An error has occurred", true);
    }
    
    [Test]
    public async Task CreateScheduledMessageThrowsException_RespondsWithErrorMessage()
    {
        const ulong userId = 1;
        Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock.Setup(u => u.GetById(userId))
            .ReturnsAsync(new DiscordUserModel { Id = userId, Username = "TestUser" });
        UnitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
        var dateString = DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:mm");
        Mock<IUser> userMock = new();
        userMock.SetupGet(x => x.Id).Returns(userId);
        InteractionContextMock.SetupGet(x => x.User).Returns(userMock.Object);
        ScheduledMessageServiceMock.Setup(s => s.CreateScheduledMessage(It.IsAny<string>(), It.IsAny<DiscordUserModel>(), It.IsAny<DateTime>(), It.IsAny<ulong>()))
            .ThrowsAsync(new Exception("Database error"));
        
        await BrobotModule.Reminder(dateString, "Test reminder");
        
        AssertRespondAsyncCalledOnce("Failed to create reminder", true);
    }
    
    [Test]
    public async Task CreateScheduledMessageSucceeds_RespondsWithSuccessMessage()
    {
        const ulong userId = 1;
        Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
        DiscordUserModel testUser = new()
        {
            Id = userId,
            Username = "TestUser"
        };
        userRepositoryMock.Setup(u => u.GetById(userId))
            .ReturnsAsync(testUser);
        UnitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
        var dateString = DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:mm");
        Mock<IUser> userMock = new();
        userMock.SetupGet(x => x.Id).Returns(userId);
        InteractionContextMock.SetupGet(x => x.User).Returns(userMock.Object);
        Mock<ITextChannel> channelMock = new();
        channelMock.SetupGet(x => x.Id).Returns(1);
        InteractionContextMock.SetupGet(x => x.Channel).Returns(channelMock.Object);
        ScheduledMessageServiceMock.Setup(s => s.CreateScheduledMessage(It.IsAny<string>(), It.IsAny<DiscordUserModel>(), It.IsAny<DateTime>(), It.IsAny<ulong>()))
            .ReturnsAsync(new ScheduledMessageModel
            {
                Id = 1,
                MessageText = "Test reminder",
                Channel = new ChannelModel
                {
                    Id = 1,
                    Name = "TestChannel",
                    Guild = new GuildModel
                    {
                        Id = 1,
                        Name = "TestGuild"
                    }
                },
                CreatedBy = testUser
            });
        
        await BrobotModule.Reminder(dateString, "Test reminder");
        
        AssertRespondAsyncCalledOnce("Reminder has been created");
    }
}
