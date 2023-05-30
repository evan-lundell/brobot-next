using System.ComponentModel.DataAnnotations;

namespace Brobot.Shared.Attributes;

public class UppercaseRequiredAttribute : RegularExpressionAttribute
{
    public UppercaseRequiredAttribute()
        : base(@".*[A-Z].*")
    {
    }
}