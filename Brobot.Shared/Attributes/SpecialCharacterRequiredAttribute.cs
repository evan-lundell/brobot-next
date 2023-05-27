using System.ComponentModel.DataAnnotations;

namespace Brobot.Shared.Attributes;

public class SpecialCharacterRequiredAttribute : RegularExpressionAttribute
{
    public SpecialCharacterRequiredAttribute()
        : base(@".*\W.*")
    {
    }
}