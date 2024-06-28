using TimeZoneConverter;

namespace Brobot.Tests.ScheduledMessageServiceTests;

[TestFixture]
public class UpdateScheduledMesssageTests : ScheduledMessageServiceTestBase
{
    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public async Task WhenUpdatingMessageText_UpdatesScheduledMessage(int scheduledMessageId)
    {
        var scheduledMessage = await Context.ScheduledMessages.FindAsync(scheduledMessageId);
        if (scheduledMessage == null)
        {
            throw new Exception("Scheduled message not found");
        }
        
        var updatedMessageText = "Updated Message";
        var updatedMessage = await ScheduledMessageService.UpdateScheduledMessage(1, updatedMessageText, null, null);

        var messageModel = await Context.ScheduledMessages.FindAsync(1);
        
        Assert.Multiple(() =>
        {
            Assert.That(updatedMessage, Is.Not.Null);
            Assert.That(updatedMessage!.MessageText, Is.EqualTo(updatedMessageText));
            Assert.That(messageModel, Is.Not.Null);
            Assert.That(messageModel!.MessageText, Is.EqualTo(updatedMessageText));
        });
    }
    
    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public async Task WhenUpdatingChannel_UpdatesScheduledMessage(int scheduledMessageId)
    {
        var scheduledMessage = await Context.ScheduledMessages.FindAsync(scheduledMessageId);
        if (scheduledMessage == null)
        {
            throw new Exception("Scheduled message not found");
        }
        
        var updatedChannelId = 2UL;
        var updatedMessage = await ScheduledMessageService.UpdateScheduledMessage(1, null, updatedChannelId, null);

        var messageModel = await Context.ScheduledMessages.FindAsync(1);
        
        Assert.Multiple(() =>
        {
            Assert.That(updatedMessage, Is.Not.Null);
            Assert.That(updatedMessage!.ChannelId, Is.EqualTo(updatedChannelId));
            Assert.That(messageModel, Is.Not.Null);
            Assert.That(messageModel!.ChannelId, Is.EqualTo(updatedChannelId));
        });
    }
    
    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public async Task WhenUpdatingSendDate_UpdatesScheduledMessage(int scheduledMessageId)
    {
        var scheduledMessage = await Context.ScheduledMessages.FindAsync(scheduledMessageId);
        if (scheduledMessage == null)
        {
            throw new Exception("Scheduled message not found");
        }
        var user = await Context.Users.FindAsync(scheduledMessage.CreatedById);
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
        
        var updatedMessage = await ScheduledMessageService.UpdateScheduledMessage(scheduledMessageId, null, null, sendDateTimeOffset.DateTime);

        var messageModel = await Context.ScheduledMessages.FindAsync(scheduledMessageId);
        
        Assert.Multiple(() =>
        {
            Assert.That(updatedMessage, Is.Not.Null);
            Assert.That(updatedMessage!.SendDate, Is.EqualTo(sendDateTimeOffset));
            Assert.That(messageModel, Is.Not.Null);
            Assert.That(messageModel!.SendDate, Is.EqualTo(sendDateTimeOffset));
        });
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public async Task SendDateIsInPast_ThrowsError(int scheduledMessageId)
    {
        var scheduledMessage = await Context.ScheduledMessages.FindAsync(scheduledMessageId);
        if (scheduledMessage == null)
        {
            throw new Exception("Scheduled message not found");
        }
        var user = await Context.Users.FindAsync(scheduledMessage.CreatedById);
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
        var sendDateTimeOffset = new DateTimeOffset(dateTimeUnspecified, offset).AddDays(-1);
        
        var ex = Assert.ThrowsAsync<Exception>(() => ScheduledMessageService.UpdateScheduledMessage(scheduledMessageId, null, null, sendDateTimeOffset.DateTime));
        Assert.That(ex.Message, Is.EqualTo("Send date cannot be in the past"));
    }
    
    [Test]
    public async Task ChannelDoesNotExist_ThrowsError()
    {
        var scheduledMessage = await Context.ScheduledMessages.FindAsync(1);
        if (scheduledMessage == null)
        {
            throw new Exception("Scheduled message not found");
        }
        
        var ex = Assert.ThrowsAsync<Exception>(() => ScheduledMessageService.UpdateScheduledMessage(1, null, 100, null));
        Assert.That(ex.Message, Is.EqualTo("Channel not found"));
    }
    
    [Test]
    public async Task ScheduledMessageDoesNotExist_ReturnsNull()
    {
        var updatedMessage = await ScheduledMessageService.UpdateScheduledMessage(100, "Test", 1, null);
        Assert.That(updatedMessage, Is.Null);
    }

    [Test]
    public void UpdateSentMessage_ThrowsError()
    {
        var ex = Assert.ThrowsAsync<Exception>(() => ScheduledMessageService.UpdateScheduledMessage(4, "Test", 1, null));
        Assert.That(ex.Message, Is.EqualTo("Cannot update a sent message"));
    }
}