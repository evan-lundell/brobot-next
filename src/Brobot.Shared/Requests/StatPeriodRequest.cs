namespace Brobot.Shared.Requests;

public record StatPeriodRequest(ulong ChannelId, DateOnly StartDate, DateOnly EndDate);