namespace Brobot.Tests.ScheduledMessageServiceTests;

[TestFixture]
public class GetScheduledMessagesByUserTests : ScheduledMessageServiceTestBase
{
    [Test]
    [TestCase(1UL, 2)]
    [TestCase(2UL, 2)]
    [TestCase(3UL, 1)]
    [TestCase(4UL, 0)]
    public async Task ReturnsCorrectNumberOfMessages(ulong userId, int expectedCount)
    {
        var user = await Context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        var messages = (await ScheduledMessageService.GetScheduledMessagesByUser(user, 100, 0, null, null)).ToList();
        
        Assert.That(messages, Has.Count.EqualTo(expectedCount));
    }
    
    [Test]
    public async Task WhenScheduledBeforeIsSpecified_ReturnsCorrectMessages()
    {
        var user = await Context.Users.FindAsync(2UL);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        var messages = (await ScheduledMessageService.GetScheduledMessagesByUser(user, 100, 0, DateTime.UtcNow, null)).ToList();
        
        Assert.That(messages, Has.Count.EqualTo(1));
        Assert.That(messages[0].MessageText, Is.EqualTo("Past Test Message 2"));
    }
    
    [Test]
    public async Task WhenScheduledAfterIsSpecified_ReturnsCorrectMessages()
    {
        var user = await Context.Users.FindAsync(1UL);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        var messages = (await ScheduledMessageService.GetScheduledMessagesByUser(user, 100, 0, null, DateTime.UtcNow)).ToList();
        
        Assert.That(messages, Has.Count.EqualTo(2));
        Assert.That(messages[0].MessageText, Is.EqualTo("Upcoming Test Message 1"));
        Assert.That(messages[1].MessageText, Is.EqualTo("Upcoming Test Message 3"));
    }
    
    [Test]
    public async Task WhenLimitIsSet_ReturnsCorrectNumberOfMessages()
    {
        var user = await Context.Users.FindAsync(1UL);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        var messages = (await ScheduledMessageService.GetScheduledMessagesByUser(user, 1, 0, null, null)).ToList();
        
        Assert.That(messages, Has.Count.EqualTo(1));
        Assert.That(messages[0].MessageText, Is.EqualTo("Upcoming Test Message 1"));
    }

    [Test]
    public async Task WhenSkipIsSet_ReturnsCorrectMessages()
    {
        var user = await Context.Users.FindAsync(1UL);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        var messages = (await ScheduledMessageService.GetScheduledMessagesByUser(user, 1, 1, null, null)).ToList();

        Assert.That(messages[0].MessageText, Is.EqualTo("Upcoming Test Message 3"));
    }
}