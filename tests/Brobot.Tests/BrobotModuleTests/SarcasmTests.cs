using Discord;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class SarcasmTests : BrobotModuleTestBase
{
    [Test]
    [TestCase("Sarcasm", "SaRcAsM")]
    [TestCase("Mixed 1234 text", "MiXeD 1234 tExT")]
    public async Task SarcasmCalled_RespondsWithSarcasmResponse(string text, string result)
    {
        await BrobotModule.Sarcasm(text);
        
       AssertRespondAsyncCalledOnce(result);
    }
}
