using Brobot.Models;
using Discord;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

public class PresenceUpdatedTests : SyncServiceTestsBase
{
    protected override void SetupDatabase()
    {
        UserModel user = new()
        {
            Id = 1UL,
            Username = "user"
        };
        Context.Users.Add(user);
        Context.SaveChanges();
    }

    [Test]
    public async Task PresenceOnline_DoNothing()
    {
        const ulong userId = 1UL;
        Mock<IUser> userMock = new();
        Mock<IPresence> formerPresenceMock = new();
        Mock<IPresence> currentPresenceMock = new();
        currentPresenceMock.SetupGet(p => p.Status).Returns(UserStatus.Online);
        
        await SyncService.PresenceUpdated(userMock.Object, formerPresenceMock.Object, currentPresenceMock.Object);
        
        Context.ChangeTracker.Clear();
        var userModel = await Context.Users.FindAsync(userId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(userModel, Is.Not.Null);
            Assert.That(userModel!.LastOnline, Is.Null);
        }
    }
    
    [Test]
    public async Task UserNotInDatabase_DoNothing()
    {
        const ulong userId = 2UL;
        Mock<IUser> userMock = new();
        Mock<IPresence> formerPresenceMock = new();
        Mock<IPresence> currentPresenceMock = new();
        currentPresenceMock.SetupGet(p => p.Status).Returns(UserStatus.Offline);
        userMock.SetupGet(u => u.Id).Returns(userId);
        
        await SyncService.PresenceUpdated(userMock.Object, formerPresenceMock.Object, currentPresenceMock.Object); 
        
        Context.ChangeTracker.Clear();
        var userModel = await Context.Users.FindAsync(userId);
        Assert.That(userModel, Is.Null);
    }
    
    [Test]
    public async Task PresenceNotOnline_UpdateLastOnline()
    {
        const ulong userId = 1UL;
        var now = DateTimeOffset.Now;
        Mock<IUser> userMock = new();
        Mock<IPresence> formerPresenceMock = new();
        Mock<IPresence> currentPresenceMock = new();
        currentPresenceMock.SetupGet(p => p.Status).Returns(UserStatus.Offline);
        userMock.SetupGet(u => u.Id).Returns(userId);
        
        await SyncService.PresenceUpdated(userMock.Object, formerPresenceMock.Object, currentPresenceMock.Object);
        
        Context.ChangeTracker.Clear();
        var userModel = await Context.Users.FindAsync(userId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(userModel, Is.Not.Null);
            Assert.That(userModel!.LastOnline, Is.Not.Null);
            Assert.That(userModel.LastOnline, Is.Not.Null);
            Assert.That(userModel.LastOnline!.Value, Is.GreaterThanOrEqualTo(now));
        }
    }
    
    [Test]
    public async Task UserIsBot_DoNothing()
    {
        const ulong userId = 2UL;
        Mock<IUser> userMock = new();
        Mock<IPresence> formerPresenceMock = new();
        Mock<IPresence> currentPresenceMock = new();
        currentPresenceMock.SetupGet(p => p.Status).Returns(UserStatus.Offline);
        userMock.SetupGet(u => u.Id).Returns(userId);
        userMock.SetupGet(u => u.IsBot).Returns(true);
        
        await SyncService.PresenceUpdated(userMock.Object, formerPresenceMock.Object, currentPresenceMock.Object);
        
        Context.ChangeTracker.Clear();
        var userModel = await Context.Users.FindAsync(userId);
        Assert.That(userModel, Is.Null);
    }
}