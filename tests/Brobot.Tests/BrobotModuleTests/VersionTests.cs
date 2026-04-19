namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class VersionTests : BrobotModuleTestBase
{
    [Test]
    public async Task CatchesException_RespondsWithErrorMessage()
    {
        AssemblyServiceMock.Setup(a => a.GetVersionFromAssembly())
            .Throws(new Exception("Assembly service error"));

        await BrobotModule.Version();
        
        AssertRespondAsyncCalledOnce("Failed to get version of brobot", true);
    }
    
    [Test]
    public async Task GetsVersionFromService_RespondsWithVersionMessage()
    {
        const string version = "1.2.3";
        AssemblyServiceMock.Setup(a => a.GetVersionFromAssembly())
            .Returns(version);
        
        await BrobotModule.Version();
        
        AssertRespondAsyncCalledOnce($"Brobot version: {version}", true);
    }
}
