using System.ComponentModel.DataAnnotations;

namespace Brobot.Configuration;

public class ExternalApisOptions
{
    public const string SectionName = "ExternalApis";
    
    [Required]
    [Url]
    public required string GiphyBaseUrl { get; init; }
    
    [Required]
    public required string GiphyApiKey { get; init; }
    
    [Required]
    [Url]
    public required string RandomFactBaseUrl { get; init; }
    
    [Required]
    [Url]
    public required string DictionaryBaseUrl { get; init; }
    
    [Required]
    [Url]
    public required string QuickChartBaseUrl  { get; init; }
}