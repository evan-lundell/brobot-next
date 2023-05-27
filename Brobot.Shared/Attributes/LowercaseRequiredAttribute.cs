using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Brobot.Shared.Attributes;

public class LowercaseRequiredAttribute : RegularExpressionAttribute
{
    public LowercaseRequiredAttribute()
        : base(@".*[a-z].*")
    {
    }
}