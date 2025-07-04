using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class VersionController(IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var filePath = configuration["VersionFilePath"];
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