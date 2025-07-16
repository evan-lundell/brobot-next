using System.ComponentModel.DataAnnotations;

namespace Brobot.Configuration;

public class DiscordOptions
{
    public const string SectionName = "Discord";
    
    [Required]
    public required string BrobotToken { get; init; }
    
    [Required]
    public required string ClientId { get; init; }
    
    [Required]
    public required string ClientSecret { get; init; }
    
    [Required]
    [Url]
    public required string AuthorizationEndpoint { get; init; }
    
    [Required]
    [Url]
    public required string TokenEndpoint { get; init; }
    
    [Required]
    [Url]
    public required string UserInformationEndpoint { get; init; }
}
