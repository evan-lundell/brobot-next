using System.ComponentModel.DataAnnotations;

namespace Brobot.Shared.Attributes;

public class NumberRequiredAttribute : RegularExpressionAttribute
{
    public NumberRequiredAttribute()
        : base(@".*\d.*")
    {
    }
}