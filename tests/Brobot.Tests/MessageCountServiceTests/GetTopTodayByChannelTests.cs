using TimeZoneConverter;

namespace Brobot.Tests.MessageCountServiceTests;

public class GetTopTodayByChannelTests : MessageCountServiceTestBase
{
    [Test]
    public async Task UserTimezoneExists_ReturnsCorrectCounts()
    {
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);
        var user = await Context.Users.FindAsync((ulong)1);
        if (user == null)
        {
            throw new Exception("User not defined");
        }

        var counts = (await MessageCountService.GetTopTodayByChannel(user, 1)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(counts[0].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(counts[0].User.Username, Is.EqualTo("User2"));
            Assert.That(counts[0].MessageCount, Is.EqualTo(15));
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
        var counts = (await MessageCountService.GetTopTodayByChannel(user4, 1)).ToList();
        Assert.That(counts.Count, Is.EqualTo(0));
    }
}