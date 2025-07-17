using Brobot.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Brobot.Tests;

[TestFixture]
public class AssemblyServiceTests
{
    [Test]
    public void GetVersionFromAssembly_ReturnsVersion()
    {
        Mock<IWebHostEnvironment> mockEnv = new();
        mockEnv.SetupGet(x => x.EnvironmentName).Returns(Environments.Development);
        AssemblyService assemblyService = new(mockEnv.Object);

        var versionNumber = assemblyService.GetVersionFromAssembly();

        // use regex to assert version number is semantic versioning
        Assert.That(versionNumber, Does.Match(@"^v\d+\.\d+\.\d+$"));
    }
}