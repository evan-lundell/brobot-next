using Brobot.Configuration;
using Brobot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class VersionController(IOptions<GeneralOptions> generalOptions, IAssemblyService assemblyService) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var filePath = generalOptions.Value.VersionFilePath;
        var deploymentVersion = "";
        if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
        {
            deploymentVersion = System.IO.File.ReadAllText(filePath).Trim();
        }

        var assemblyVersion = assemblyService.GetVersionFromAssembly();

        return Ok(new { DeploymentVersion = deploymentVersion, AssemblyInformationalVersion = assemblyVersion });
    }
}