using TimeZoneConverter;

namespace Brobot.Tests.MessageCountServiceTests;

public class GetUsersTotalDailyMessageCountsTests : MessageCountServiceTestBase
{
    [Test]
    public async Task WhenUserTimezoneExists_ReturnsCorrectCounts()
    {
        var user = await Context.Users.FindAsync((ulong)1);
        if (user == null)
        {
            throw new Exception("User not defined");
        }

        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);

        var counts = (await MessageCountService.GetUsersTotalDailyMessageCounts(user, 10)).ToList();
        var orderedCounts = counts.OrderBy(c => c.CountDate).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(counts.Count, Is.EqualTo(10));
            Assert.That(orderedCounts[0].CountDate,
                Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-9) + offset)));
            Assert.That(orderedCounts[0].MessageCount, Is.EqualTo(0));
            Assert.That(orderedCounts[9].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(orderedCounts[9].MessageCount, Is.EqualTo(12));
            Assert.That(counts.Sum(c => c.MessageCount), Is.EqualTo(89));
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
        var counts = (await MessageCountService.GetUsersTotalDailyMessageCounts(user4, 10)).ToList();
        Assert.That(counts.Count, Is.EqualTo(0));
    }
}