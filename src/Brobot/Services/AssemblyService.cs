using System.Reflection;

namespace Brobot.Services;

public class AssemblyService(IWebHostEnvironment environment) : IAssemblyService
{
    public string GetVersionFromAssembly()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "v0.0.0";
        var index = assemblyVersion.IndexOf('+');
        if (index > 0)
        {
            assemblyVersion = assemblyVersion[..index];
        }

        return assemblyVersion;
    }
}