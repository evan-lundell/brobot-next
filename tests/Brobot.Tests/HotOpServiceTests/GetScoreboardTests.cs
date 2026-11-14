using Brobot.Models;

namespace Brobot.Tests.HotOpServiceTests;

[TestFixture]
public class GetScoreboardTests : HotOpServiceTestsBase
{
    [Test]
    public async Task ReturnsCorrectScores()
    {
        var hotOps = (await UnitOfWork.HotOps.GetActiveHotOpsWithSessions(1)).ToList();
        var scoreboard = HotOpService.GetScoreboard(hotOps[0]);
        Assert.Multiple(() =>
        {
            Assert.That(hotOps.Count, Is.EqualTo(1));
            Assert.That(scoreboard.Scores.Count(), Is.EqualTo(2));
            Assert.That(scoreboard.OwnerUsername, Is.EqualTo("Test User 1"));
            Assert.That(scoreboard.Scores.First().Username, Is.EqualTo("Test User 2"));
            Assert.That(scoreboard.Scores.First().Score, Is.EqualTo(1000));
            Assert.That(scoreboard.Scores.Last().Username, Is.EqualTo("Test User 3"));
            Assert.That(scoreboard.Scores.Last().Score, Is.EqualTo(400));
        });
    }

    [Test]
    public async Task ReturnsZeroScoreWhenNoSessions()
    {
        var hotOp = (await UnitOfWork.HotOps.GetActiveHotOpsWithSessions(2)).First();
        var scoreboard = HotOpService.GetScoreboard(hotOp);
        
        Assert.That(scoreboard.Scores.First().Score, Is.EqualTo(0));
    }

    [Test]
    public async Task IgnoresUserNotInChannel()
    {
        var hotOp = await GetHotOp(5);
        
        var scoreboard = HotOpService.GetScoreboard(hotOp);
        
        Assert.Multiple(() =>
        {
            Assert.That(scoreboard.Scores.Count(), Is.EqualTo(1));
            Assert.That(scoreboard.Scores.First().Username, Is.EqualTo("Test User 2"));
        });
    }

    [Test]
    public async Task OpenSessionOnEndedHotOp_IsCappedAtEndDate()
    {
        // HotOp 2 ended 8 days ago. User 1 already has a closed 120 minute session (1200 points).
        // Simulate a session that was never closed (e.g. the end-of-hot-op race) that started 60 minutes before the end.
        var hotOp = await GetHotOp(2);
        var user1 = hotOp.Channel.ChannelUsers.Single(cu => cu.UserId == 1UL).DiscordUser;
        hotOp.HotOpSessions.Add(new HotOpSessionModel
        {
            HotOp = hotOp,
            HotOpId = hotOp.Id,
            DiscordUser = user1,
            DiscordUserId = user1.Id,
            StartDateTime = hotOp.EndDate.AddMinutes(-60),
            EndDateTime = null
        });

        var scoreboard = HotOpService.GetScoreboard(hotOp);

        // 1200 + 600 (capped at EndDate). Uncapped would be 1200 + ~8 days' worth.
        Assert.That(scoreboard.Scores.Single(s => s.UserId == 1UL).Score, Is.EqualTo(1800));
    }
}