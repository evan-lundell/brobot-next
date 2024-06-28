namespace Brobot.Tests.ScheduledMessageServiceTests;

[TestFixture]
public class DeleteScheduledMessageTests : ScheduledMessageServiceTestBase
{
    [Test]
    public async Task WhenMessageExists_MessageIsDeleted()
    {
        var message = await Context.ScheduledMessages.FindAsync(1);
        if (message == null)
        {
            throw new Exception("Message not found");
        }

        await ScheduledMessageService.DeleteScheduledMessage(message.Id);

        var deletedMessage = await Context.ScheduledMessages.FindAsync(1);
        Assert.That(deletedMessage, Is.Null);
    }
    
    [Test]
    public async Task WhenMessageDoesNotExist_ReturnsFalse()
    {
        var result = await ScheduledMessageService.DeleteScheduledMessage(100);
        Assert.That(result, Is.False);
    }
}