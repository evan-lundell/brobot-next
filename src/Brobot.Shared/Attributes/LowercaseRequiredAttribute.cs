using System.ComponentModel.DataAnnotations;

namespace Brobot.Shared.Attributes;

public class LowercaseRequiredAttribute : RegularExpressionAttribute
{
    public LowercaseRequiredAttribute()
        : base(@".*[a-z].*")
    {
    }
}