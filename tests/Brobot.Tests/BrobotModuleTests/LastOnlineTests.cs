using Brobot.Models;
using Brobot.Repositories;
using Discord;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class LastOnlineTests : BrobotModuleTestBase
{
    [Test]
    public async Task UserIsOnline_RespondsWithOnlineNowMessage()
    {
        Mock<IUser> userMock = new();
        userMock.SetupGet(u => u.Status).Returns(UserStatus.Online);
        userMock.SetupGet(u => u.Username).Returns("TestUser");
        
        await BrobotModule.LastOnline(userMock.Object);

        AssertRespondAsyncCalledOnce("TestUser is online now!", true);
    }

    [Test]
    public async Task LastOnlineNotFound_RespondsWithNotFoundMessage()
    {
        const ulong userId = 1;
        const ulong callingUserId = 2;
        Mock<IUser> userMock = new();
        userMock.SetupGet(u => u.Status).Returns(UserStatus.Offline);
        userMock.SetupGet(u => u.Id).Returns(userId);
        Mock<IUser> callingUserMock = new();
        callingUserMock.SetupGet(u => u.Id).Returns(callingUserId);
        InteractionContextMock.SetupGet(c => c.User).Returns(callingUserMock.Object);
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock.Setup(u => u.GetById(userId))
            .ReturnsAsync(new DiscordUserModel
            {
                Id = userId,
                Username = "TestUser",
                LastOnline = null
            });
        UnitOfWorkMock.SetupGet(uow => uow.Users).Returns(userRepositoryMock.Object);
        
        await BrobotModule.LastOnline(userMock.Object);
        
        AssertRespondAsyncCalledOnce("Failed to get last online", true);
    }

    [Test]
    public async Task FindsLastOnline_RespondsWithLastOnlineMessage()
    {
        const ulong userId = 1;
        const ulong callingUserId = 2;
        Mock<IUser> userMock = new();
        userMock.SetupGet(u => u.Status).Returns(UserStatus.Offline);
        userMock.SetupGet(u => u.Id).Returns(userId);
        Mock<IUser> callingUserMock = new();
        callingUserMock.SetupGet(u => u.Id).Returns(callingUserId);
        InteractionContextMock.SetupGet(c => c.User).Returns(callingUserMock.Object);
        Mock<IUserRepository> userRepositoryMock = new();
        var lastOnline = new DateTime(2026, 4, 19, 12, 0, 0);
        const string timezone = "America/Chicago";
        var lastOnlineAdjusted = new DateTime(2026, 4, 19, 7, 0, 0);
        userRepositoryMock.Setup(u => u.GetById(userId))
            .ReturnsAsync(new DiscordUserModel
            {
                Id = userId,
                Username = "TestUser",
                LastOnline = lastOnline,
            });
        userRepositoryMock.Setup(u => u.GetById(callingUserId))
            .ReturnsAsync(new DiscordUserModel
            {
                Id = callingUserId,
                Username = "CallingUser",
                Timezone = timezone
            });
        UnitOfWorkMock.SetupGet(uow => uow.Users).Returns(userRepositoryMock.Object);
        
        await BrobotModule.LastOnline(userMock.Object);
        
        AssertRespondAsyncCalledOnce($"TestUser was last online at {lastOnlineAdjusted:yyyy-MM-dd hh:mm tt}", true);
    }
    
    [Test]
    public async Task ExceptionOccurs_RespondsWithErrorMessage()
    {
        Mock<IUser> userMock = new();
        userMock.SetupGet(u => u.Status).Returns(UserStatus.Offline);
        userMock.SetupGet(u => u.Username).Returns("TestUser");
        UnitOfWorkMock.Setup(u => u.Users).Throws(new Exception("Database error"));
        
        await BrobotModule.LastOnline(userMock.Object);
        
        AssertRespondAsyncCalledOnce("Failed to get last online", true);
    }
}