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

    [Test]
    public async Task WhenMessageDoesNotExist_ReturnsFalse()
    {
        var user = await Context.DiscordUsers.FindAsync(1UL);

        var result = await ScheduledMessageService.CanUserUpdateScheduledMessage(user!, 999);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task WhenUserIsNotOwner_ReturnsFalse()
    {
        // Message 1 was created by User 1
        var user = await Context.DiscordUsers.FindAsync(2UL);

        var result = await ScheduledMessageService.CanUserUpdateScheduledMessage(user!, 1);

        Assert.That(result, Is.False);
    }
}
