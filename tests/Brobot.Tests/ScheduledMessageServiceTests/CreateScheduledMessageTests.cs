using TimeZoneConverter;

namespace Brobot.Tests.ScheduledMessageServiceTests;

[TestFixture]
public class CreateScheduledMessageTests : ScheduledMessageServiceTestBase
{
    [Test]
    [TestCase(1UL)]
    [TestCase(2UL)]
    public async Task WhenMessageIsValid_MessageIsScheduled(ulong userId)
    {
        var user = await Context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        var offset = TimeSpan.FromHours(0);
        if (!string.IsNullOrWhiteSpace(user.Timezone))
        {
            var timezone = TZConvert.GetTimeZoneInfo(user.Timezone);
            offset = timezone.GetUtcOffset(DateTime.Now);
        }

        var dateTimeUnspecified = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        var sendDateTimeOffset = new DateTimeOffset(dateTimeUnspecified, offset).AddDays(1);
        
        var message =
            await ScheduledMessageService.CreateScheduledMessage("New Message", user, sendDateTimeOffset.DateTime, 1);
        
        Assert.Multiple(() =>
        {
            Assert.That(message, Is.Not.Null);
            Assert.That(message.MessageText, Is.EqualTo("New Message"));
            Assert.That(message.Id, Is.EqualTo(6));
            Assert.That(message.SendDate, Is.Not.Null);
            Assert.That(message.SendDate, Is.EqualTo(sendDateTimeOffset));
            Assert.That(message.SentDate, Is.Null);
        });
    }

    [Test]
    public async Task WhenMessageIsScheduledInPast_ThrowsError()
    {
        var user = await Context.Users.FindAsync(1UL);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        var ex = Assert.ThrowsAsync<Exception>(
            () => ScheduledMessageService.CreateScheduledMessage("New Message", user, DateTime.UtcNow.AddDays(-1), 1));
        Assert.That(ex.Message, Is.EqualTo("Send date cannot be in the past"));
    }
    
    [Test]
    public async Task WhenChannelDoesNotExist_ThrowsError()
    {
        var user = await Context.Users.FindAsync(1UL);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        var ex = Assert.ThrowsAsync<Exception>(
            () => ScheduledMessageService.CreateScheduledMessage("New Message", user, DateTime.UtcNow.AddDays(1), 100));
        Assert.That(ex.Message, Is.EqualTo("Channel not found"));
    }
}