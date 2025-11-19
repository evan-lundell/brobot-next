using Brobot.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Brobot.Tests;

[TestFixture]
public class AssemblyServiceTests
{
    [Test]
    public void GetVersionFromAssembly_ReturnsVersion()
    {
        
        AssemblyService assemblyService = new(Mock.Of<ILogger<AssemblyService>>());

        var versionNumber = assemblyService.GetVersionFromAssembly();

        // use regex to assert version number is semantic versioning
        Assert.That(versionNumber, Does.Match(@"^\d+\.\d+\.\d+$"));
    }
}