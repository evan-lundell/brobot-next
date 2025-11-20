using System.ComponentModel.DataAnnotations;

namespace Brobot.Configuration;

public class GeneralOptions
{
    public const string SectionName = "General";

    public bool FixTwitterLinks { get; init; }
    
    [Required]
    public required string VersionFilePath { get; init; }
    
    public string? ReleaseNotesUrl { get; init; }
    
    [Required]
    public required string SeqUrl { get; init; }
}