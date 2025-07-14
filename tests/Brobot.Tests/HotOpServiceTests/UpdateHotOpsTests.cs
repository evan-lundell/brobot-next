using Brobot.Shared;

namespace Brobot.Tests.HotOpServiceTests;

[TestFixture]
public class UpdateHotOpsTests : HotOpServiceTestsBase
{
    [Test]
    public async Task OwnerJoinsChannel_SessionsAreCreated()
    {
        await HotOpService.UpdateHotOps(2UL, UserVoiceStateAction.Connected, [1UL, 2UL]);

        var hotOp = await UnitOfWork.HotOps.GetById(4);
        Assert.Multiple(() =>
        {
            Assert.That(hotOp!.HotOpSessions, Has.Count.EqualTo(1));
            Assert.That(hotOp.HotOpSessions.First().UserId, Is.EqualTo(1UL));
            Assert.That(DateTimeOffset.Now - hotOp.HotOpSessions.First().StartDateTime, Is.LessThan(TimeSpan.FromSeconds(5)));
            Assert.That(hotOp.HotOpSessions.First().EndDateTime, Is.Null);
        });
    }

    [Test]
    public async Task OwnerLeavesChannel_SessionsAreEnded()
    {
        await HotOpService.UpdateHotOps(1UL, UserVoiceStateAction.Disconnected, [3UL]);

        var hotOp = await UnitOfWork.HotOps.GetById(1);
        Assert.Multiple(() =>
        {
            Assert.That(hotOp!.HotOpSessions, Has.Count.EqualTo(4));
            Assert.That(hotOp.HotOpSessions.Any(hos => hos.EndDateTime == null), Is.False);
        });
    }
    
    [Test]
    public async Task UserJoinsChannel_SessionIsCreated()
    {
        await HotOpService.UpdateHotOps(1UL, UserVoiceStateAction.Connected, [1UL, 2UL]);

        var hotOp = await UnitOfWork.HotOps.GetById(4);
        Assert.Multiple(() =>
        {
            Assert.That(hotOp!.HotOpSessions, Has.Count.EqualTo(1));
            Assert.That(hotOp.HotOpSessions.First().UserId, Is.EqualTo(1UL));
            Assert.That(DateTimeOffset.UtcNow - hotOp.HotOpSessions.First().StartDateTime, Is.LessThan(TimeSpan.FromSeconds(5)));
            Assert.That(hotOp.HotOpSessions.First().EndDateTime, Is.Null);
        });
    }

    [Test]
    public async Task UserLeavesChannel_SessionIsEnded()
    {
        await HotOpService.UpdateHotOps(3UL, UserVoiceStateAction.Disconnected, [1UL]);

        var hotOp = await UnitOfWork.HotOps.GetById(1);
        Assert.Multiple(() =>
        {
            Assert.That(hotOp!.HotOpSessions, Has.Count.EqualTo(4));
            Assert.That(hotOp.HotOpSessions.Any(hos => hos.EndDateTime == null), Is.False);
        });
    }

    [Test]
    [TestCase(UserVoiceStateAction.Connected)]
    [TestCase(UserVoiceStateAction.Disconnected)]
    public async Task ChannelWithoutOwner_NoChangesToHotOps(UserVoiceStateAction action)
    {
        await HotOpService.UpdateHotOps(4UL, action, [4UL, 5UL]);

        var hotOps = (await UnitOfWork.HotOps.GetAll()).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(hotOps.First(ho => ho.Id == 1).HotOpSessions, Has.Count.EqualTo(4));
            Assert.That(hotOps.First(ho => ho.Id == 2).HotOpSessions, Has.Count.EqualTo(1));
            Assert.That(hotOps.First(ho => ho.Id == 3).HotOpSessions, Is.Empty);
            Assert.That(hotOps.First(ho => ho.Id == 4).HotOpSessions, Is.Empty);
            Assert.That(hotOps.First(ho => ho.Id == 5).HotOpSessions, Has.Count.EqualTo(2));
        });
    }
}