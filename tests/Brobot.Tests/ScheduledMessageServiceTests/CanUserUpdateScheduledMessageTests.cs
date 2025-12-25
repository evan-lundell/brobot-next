namespace Brobot.Tests.ScheduledMessageServiceTests;

[TestFixture]
public class CanUserUpdateScheduledMessageTests : ScheduledMessageServiceTestBase
{
    [Test]
    public async Task WhenMessageExistsAndUserIsOwner_ReturnsTrue()
    {
        var message = await Context.ScheduledMessages.FindAsync(1);
        if (message == null)
        {
            throw new Exception("Message not found");
        }

        var user = await Context.DiscordUsers.FindAsync(1UL);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        var result = await ScheduledMessageService.CanUserUpdateScheduledMessage(user, message.Id);
        Assert.That(result, Is.True);
    }
}