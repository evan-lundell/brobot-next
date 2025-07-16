using System.ComponentModel.DataAnnotations;

namespace Brobot.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    
    [Required]
    public required string SigningKey { get; init; }
    
    [Required]
    public required string ValidIssuer { get; init; }
    
    [Required]
    public required string ValidAudience { get; init; }
    
    [Required]
    public int Expiry { get; init; }
}