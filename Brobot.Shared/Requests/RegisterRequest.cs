using System.ComponentModel.DataAnnotations;
using Brobot.Shared.Attributes;

namespace Brobot.Shared.Requests;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public required string EmailAddress { get; set; }

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [LowercaseRequired(ErrorMessage = "Password must contain at least one lowercase letter")]
    [UppercaseRequired(ErrorMessage = "Password must contain at least one uppercase letter")]
    [NumberRequired(ErrorMessage = "Password must contain at least one number")]
    [SpecialCharacterRequired(ErrorMessage = "Password must contain at least one special character")]
    public required string Password { get; set; }

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public required string ConfirmPassword { get; set; }
}