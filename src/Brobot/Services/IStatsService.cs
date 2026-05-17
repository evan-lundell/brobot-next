using Brobot.Dtos;
using Brobot.Models;

namespace Brobot.Services;

public interface IStatsService
{
    Task<StatsDto> GetStats(ChannelModel channel, DateOnly startDate, DateOnly endDate, int? statPeriodId = null, CancellationToken cancellationToken = default);
    Task SendStats(ulong channelId, StatsDto stats, CancellationToken cancellationToken = default);
}