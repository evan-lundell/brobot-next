using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Services;

public interface IMessageCountService
{
    Task<IEnumerable<DailyMessageCountResponse>> GetUsersDailyMessageCountForChannel(ulong userId, ulong channelId, int numOfDays);
    Task<IEnumerable<DailyMessageCountResponse>> GetUsersTotalDailyMessageCounts(UserModel userModel, int numOfDays);
    Task AddToDailyCount(ulong userId, ulong channelId, DateOnly? countDate = null);
    Task<IEnumerable<DailyMessageCountResponse>> GetUsersTopDays(UserModel userModel, int numOfDays);
    Task<IEnumerable<DailyMessageCountResponse>> GetUsersTopDaysByChannel(UserModel userModel, ulong channelId, int numOfDays);
    Task<IEnumerable<DailyMessageCountResponse>> GetTopToday(UserModel userModel);
    Task<IEnumerable<DailyMessageCountResponse>> GetTopTodayByChannel(UserModel userModel, ulong channelId);
    Task<IEnumerable<DailyMessageCountResponse>> GetTotalDailyMessageCounts(int numOfDays, string? usersTimezone);
    Task<IEnumerable<DailyMessageCountResponse>> GetTotalDailyMessageCountsByChannel(int numOfDays, ulong channelId, string? usersTimezone);
    Task<IEnumerable<DailyMessageCountResponse>> GetTotalTopDays(int numOfDays);
    Task<IEnumerable<DailyMessageCountResponse>> GetTotalTopDaysByChannel(ulong channelId, int numOfDays);
}