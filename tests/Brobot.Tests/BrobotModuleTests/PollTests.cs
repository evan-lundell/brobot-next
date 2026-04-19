using Discord;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class PollTests : BrobotModuleTestBase
{
    [Test]
    public async Task PollCalled_RespondsWithPollMessage()
    {
        await BrobotModule.Poll(
            "Poll question", 
            "Option1", 
            "Option2", 
            "Option3",
            "Option4",
            "Option5",
            "Option6",
            "Option7",
            "Option8",
            "Option9");
        
        AssertRespondAsyncCalledOnce("**Poll question**\n\n:one:: Option1\n:two:: Option2\n:three:: Option3\n:four:: Option4\n:five:: Option5\n:six:: Option6\n:seven:: Option7\n:eight:: Option8\n:nine:: Option9\n");
    }
}
