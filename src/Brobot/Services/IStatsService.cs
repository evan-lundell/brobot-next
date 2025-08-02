using Brobot.Dtos;
using Brobot.Models;

namespace Brobot.Services;

public interface IStatsService
{
    Task<StatsDto> GetStats(ChannelModel channel, DateOnly startDate, DateOnly endDate);
    Task SendStats(ulong channelId, StatsDto stats);
}