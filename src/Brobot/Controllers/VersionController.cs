using System.Reflection;
using Brobot.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class VersionController(IOptions<GeneralOptions> generalOptions) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var filePath = generalOptions.Value.VersionFilePath;
        var deploymentVersion = "";
        if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
        {
            deploymentVersion = System.IO.File.ReadAllText(filePath);
        }
        var assembly = Assembly.GetExecutingAssembly();
        
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        return Ok(new { DeploymentVersion = deploymentVersion, AssemblyInformationalVersion = informationalVersion });
    }
}