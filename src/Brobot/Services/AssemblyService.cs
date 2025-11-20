using System.Reflection;

namespace Brobot.Services;

public class AssemblyService(ILogger<AssemblyService> logger) : IAssemblyService
{
    public string GetVersionFromAssembly()
    {
        logger.LogInformation("Getting version from assembly");
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "v0.0.0";
        var index = assemblyVersion.IndexOf('+');
        if (index > 0)
        {
            assemblyVersion = assemblyVersion[..index];
        }

        logger.LogInformation("Found version {AssemblyVersion}", assemblyVersion);
        return assemblyVersion;
    }
}