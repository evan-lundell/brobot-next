using Discord;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class FactTests : BrobotModuleTestBase
{
    [Test]
    public async Task RandomFactServiceThrowsException_RespondsWithErrorMessage()
    {
        RandomFactServiceMock.Setup(r => r.GetFact())
            .Throws(new Exception("RandomFactServiceThrowsException"));

        await BrobotModule.Fact();
        
        AssertRespondAsyncCalledOnce("An error occurred");
    }

    [Test]
    public async Task RandomFactServiceSucceeds_RespondsWithRandomFact()
    {
        RandomFactServiceMock.Setup(r => r.GetFact())
            .ReturnsAsync("Random fact");
        
        await BrobotModule.Fact();
        
        AssertRespondAsyncCalledOnce("Random fact");
    }
}