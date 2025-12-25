using System.ComponentModel.DataAnnotations;

namespace Brobot.Shared.Requests;

public record DiscordLoginRequest
{
    [Required]
    public required string AuthorizationCode { get; init; }
    
    [Required]
    public required string RedirectUri { get; init; }
    
    [Required]
    public required string State { get; init; }
}

