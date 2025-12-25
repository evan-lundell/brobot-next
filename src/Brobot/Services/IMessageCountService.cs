using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Services;

public interface IMessageCountService
{
    Task<IEnumerable<DailyMessageCountResponse>> GetUsersDailyMessageCountForChannel(ulong userId, ulong channelId, int numOfDays);
    Task<IEnumerable<DailyMessageCountResponse>> GetUsersTotalDailyMessageCounts(DiscordUserModel discordUserModel, int numOfDays);
    Task AddToDailyCount(ulong userId, ulong channelId, DateOnly? countDate = null);
    Task<IEnumerable<DailyMessageCountResponse>> GetUsersTopDays(DiscordUserModel discordUserModel, int numOfDays);
    Task<IEnumerable<DailyMessageCountResponse>> GetUsersTopDaysByChannel(DiscordUserModel discordUserModel, ulong channelId, int numOfDays);
    Task<IEnumerable<DailyMessageCountResponse>> GetTopToday(DiscordUserModel discordUserModel);
    Task<IEnumerable<DailyMessageCountResponse>> GetTopTodayByChannel(DiscordUserModel discordUserModel, ulong channelId);
    Task<IEnumerable<DailyMessageCountResponse>> GetTotalDailyMessageCounts(int numOfDays, string? usersTimezone);
    Task<IEnumerable<DailyMessageCountResponse>> GetTotalDailyMessageCountsByChannel(int numOfDays, ulong channelId, string? usersTimezone);
    Task<IEnumerable<DailyMessageCountResponse>> GetTotalTopDays(int numOfDays);
    Task<IEnumerable<DailyMessageCountResponse>> GetTotalTopDaysByChannel(ulong channelId, int numOfDays);
}