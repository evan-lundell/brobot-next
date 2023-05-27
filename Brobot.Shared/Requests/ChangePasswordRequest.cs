using System.ComponentModel.DataAnnotations;
using Brobot.Shared.Attributes;

namespace Brobot.Shared.Requests;

public class ChangePasswordRequest
{
    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [LowercaseRequired(ErrorMessage = "Password must contain at least one lowercase letter")]
    [UppercaseRequired(ErrorMessage = "Password must contain at least one uppercase letter")]
    [NumberRequired(ErrorMessage = "Password must contain at least one number")]
    [SpecialCharacterRequired(ErrorMessage = "Password must contain at least one special character")]
    public required string CurrentPassword { get; set; }
    
    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [LowercaseRequired(ErrorMessage = "Password must contain at least one lowercase letter")]
    [UppercaseRequired(ErrorMessage = "Password must contain at least one uppercase letter")]
    [NumberRequired(ErrorMessage = "Password must contain at least one number")]
    [SpecialCharacterRequired(ErrorMessage = "Password must contain at least one special character")]
    public required string NewPassword { get; set; }

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public required string ConfirmNewPassword { get; set; }
}