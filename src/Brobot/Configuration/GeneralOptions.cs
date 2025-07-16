using System.ComponentModel.DataAnnotations;

namespace Brobot.Configuration;

public class GeneralOptions
{
    public const string SectionName = "General";

    public bool FixTwitterLinks { get; init; } = false;
    
    [Required]
    public required string VersionFilePath { get; init; }
}