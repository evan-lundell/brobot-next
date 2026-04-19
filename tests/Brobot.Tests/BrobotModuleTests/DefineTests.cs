using Discord;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class DefineTests : BrobotModuleTestBase
{
    [Test]
    public async Task DictionaryServiceThrowsException_RespondsWithErrorMessage()
    {
        DictionaryServiceMock.Setup(r => r.GetDefinition("Word"))
            .Throws(new Exception("DictionaryServiceThrowsException"));

        await BrobotModule.Define("Word");
        
        AssertRespondAsyncCalledOnce("An error occurred");
    }

    [Test]
    public async Task DictionaryServiceSucceeds_RespondsWithRandomFact()
    {
        DictionaryServiceMock.Setup(r => r.GetDefinition("Word"))
            .ReturnsAsync("Definition of Word");
        
        await BrobotModule.Define("Word");
        
        AssertRespondAsyncCalledOnce("Definition of Word");
    }
}
