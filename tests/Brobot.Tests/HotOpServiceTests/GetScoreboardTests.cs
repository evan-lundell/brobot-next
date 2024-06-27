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
}