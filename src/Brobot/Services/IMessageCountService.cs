using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Services;

public interface IMessageCountService
{
    Task<IEnumerable<DailyMessageCountResponse>> GetUsersDailyMessageCountForChannel(ulong userId, ulong channelId, int numOfDays, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountResponse>> GetUsersTotalDailyMessageCounts(DiscordUserModel discordUserModel, int numOfDays, CancellationToken cancellationToken = default);
    Task AddToDailyCount(ulong userId, ulong channelId, DateOnly? countDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountResponse>> GetUsersTopDays(DiscordUserModel discordUserModel, int numOfDays, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountResponse>> GetUsersTopDaysByChannel(DiscordUserModel discordUserModel, ulong channelId, int numOfDays, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountResponse>> GetTopToday(DiscordUserModel discordUserModel, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountResponse>> GetTopTodayByChannel(DiscordUserModel discordUserModel, ulong channelId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountResponse>> GetTotalDailyMessageCounts(int numOfDays, string? usersTimezone, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountResponse>> GetTotalDailyMessageCountsByChannel(int numOfDays, ulong channelId, string? usersTimezone, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountResponse>> GetTotalTopDays(int numOfDays, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountResponse>> GetTotalTopDaysByChannel(ulong channelId, int numOfDays, CancellationToken cancellationToken = default);
}