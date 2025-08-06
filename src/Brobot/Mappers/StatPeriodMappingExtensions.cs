using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class StatPeriodMappingExtensions
{
    public static StatPeriodResponse ToStatPeriodResponse(this StatPeriodModel model)
    {
        return new StatPeriodResponse
        {
            Id = model.Id,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            WordCounts = model.WordCounts.Select(wc => wc.ToWordCountResponse()),
            UserMessageCounts = model.UserMessageCounts.Select(umc => umc.ToUserMessageCountResponse())
        };
    }
}