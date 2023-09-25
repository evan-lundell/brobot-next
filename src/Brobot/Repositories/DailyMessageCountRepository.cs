using Brobot.Contexts;
using Brobot.Models;
using Discord;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class DailyMessageCountRepository : RepositoryBase<DailyMessageCountModel, (ulong, ulong, DateOnly)>, IDailyMessageCountRepository
{
    public DailyMessageCountRepository(BrobotDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<DailyMessageCountModel>> GetUsersTopDays(ulong userId, int numOfDays)
    {
        return await Context.DailyMessageCounts
            .FromSqlRaw(
                $"SELECT user_id, 0 as channel_id, count_date, SUM(message_count) AS message_count FROM daily_message_count WHERE user_id = {userId} GROUP BY user_id, count_date ORDER BY message_count DESC LIMIT {numOfDays}")
            .Include((dmc) => dmc.User)
            .OrderByDescending((dmc) => dmc.MessageCount)
            .ToListAsync();
    }


    public async Task<IEnumerable<DailyMessageCountModel>> GetUsersTopDaysInChannel(ulong userId, ulong channelId, int numOfDays)
    {
        return await Context.DailyMessageCounts
            .FromSqlRaw(
                $"SELECT user_id, channel_id, count_date, SUM(message_count) AS message_count FROM daily_message_count WHERE user_id = {userId} AND channel_id = {channelId} GROUP BY user_id, channel_id, count_date ORDER BY message_count DESC LIMIT {numOfDays}")
            .Include((dmc) => dmc.User)
            .OrderByDescending((dmc) => dmc.MessageCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<DailyMessageCountModel>> GetTopForDate(DateOnly date)
    {
        return await Context.DailyMessageCounts
            .FromSqlRaw(
                $"SELECT user_id, 0 AS channel_id, count_date, SUM(message_count) AS message_count FROM daily_message_count WHERE count_date = '{date.ToString()}' GROUP BY user_id, count_date ORDER BY message_count DESC")
            .Include((dmc) => dmc.User)
            .OrderByDescending((dmc) => dmc.MessageCount)
            .ToListAsync();
    }


    public async Task<IEnumerable<DailyMessageCountModel>> GetTopForDateByChannel(DateOnly date, ulong channelId)
    {
        return await Context.DailyMessageCounts
            .FromSqlRaw(
                $"SELECT user_id, channel_id, count_date, message_count FROM daily_message_count WHERE count_date = '{date.ToString()}' AND channel_id = {channelId} ORDER BY message_count DESC")
            .Include((dmc) => dmc.User)
            .OrderByDescending((dmc) => dmc.MessageCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<DailyMessageCountModel>> GetTotalDailyMessageCounts(DateOnly startDate, DateOnly endDate)
    {
        return await Context.DailyMessageCounts
            .FromSqlRaw(
                $"SELECT 0 as user_id, 0 as channel_id, count_date, SUM(message_count) AS message_count FROM daily_message_count WHERE count_date >= '{startDate.ToString("yyyy-MM-dd")}' AND count_date <= '{endDate.ToString("yyyy-MM-dd")}' GROUP BY count_date ORDER BY count_date DESC")
            .ToListAsync();
    }
    
    public async Task<IEnumerable<DailyMessageCountModel>> GetTotalDailyMessageCountsByChannel(DateOnly startDate, DateOnly endDate, ulong channelId)
    {
        return await Context.DailyMessageCounts
            .FromSqlRaw(
                $"SELECT 0 as user_id, channel_id, count_date, SUM(message_count) AS message_count FROM daily_message_count WHERE count_date >= '{startDate.ToString("yyyy-MM-dd")}' AND count_date <= '{endDate.ToString("yyyy-MM-dd")}' AND channel_id = {channelId} GROUP BY count_date, channel_id ORDER BY count_date DESC")
            .ToListAsync();
    }

    public async Task<IEnumerable<DailyMessageCountModel>> GetTotalTopDays(int numOfDays)
    {
        return await Context.DailyMessageCounts
            .FromSqlRaw(
                $"SELECT 0 AS user_id, 0 AS channel_id, count_date, SUM(message_count) AS message_count FROM daily_message_count GROUP BY count_date ORDER BY message_count DESC LIMIT {numOfDays}"
            )
            .ToListAsync();
    }
    
    public async Task<IEnumerable<DailyMessageCountModel>> GetTotalTopDaysByChannel(ulong channelId, int numOfDays)
    {
        return await Context.DailyMessageCounts
            .FromSqlRaw(
                $"SELECT 0 AS user_id, channel_id, count_date, SUM(message_count) AS message_count FROM daily_message_count WHERE channel_id = {channelId} GROUP BY channel_id, count_date ORDER BY message_count DESC LIMIT {numOfDays}"
            )
            .ToListAsync();
    }
}