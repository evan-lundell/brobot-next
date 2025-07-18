using Brobot.Services;

namespace Brobot.Tests;

[TestFixture]
public class AssemblyServiceTests
{
    [Test]
    public void GetVersionFromAssembly_ReturnsVersion()
    {
        AssemblyService assemblyService = new();

        var versionNumber = assemblyService.GetVersionFromAssembly();

        // use regex to assert version number is semantic versioning
        Assert.That(versionNumber, Does.Match(@"^v\d+\.\d+\.\d+$"));
    }
}