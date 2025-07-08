namespace Brobot.Tests.MessageCountServiceTests;

public class GetTopTodayTests : MessageCountServiceTestBase
{
    [Test]
    public async Task WhenUserTimezoneExists_ReturnsCorrectCounts()
    {
        var user2 = await Context.Users.FindAsync(2UL);
        if (user2 == null)
        {
            throw new Exception("User2 not defined");
        }
        var user1 = await Context.Users.FindAsync(1UL);
        if (user1 == null)
        {
            throw new Exception("User1 not defined");
        }

        var counts = (await MessageCountService.GetTopToday(user2)).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(counts[0].User.Username, Is.EqualTo(user2.Username));
            Assert.That(counts[0].MessageCount, Is.EqualTo(18));
            Assert.That(counts[1].User.Username, Is.EqualTo(user1.Username));
            Assert.That(counts[1].MessageCount, Is.EqualTo(12));
            Assert.That(counts[2].MessageCount, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task WhenUserHasNoTimezone_ReturnsEmptyArray()
    {
        var user4 = await Context.Users.FindAsync(4UL);
        if (user4 == null)
        {
            throw new Exception("User4 not defined");
        }
        var counts = (await MessageCountService.GetTopToday(user4)).ToList();
        Assert.That(counts.Count, Is.EqualTo(0));
    }
}