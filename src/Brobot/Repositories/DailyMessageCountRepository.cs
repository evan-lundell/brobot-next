using Brobot.Contexts;
using Brobot.Models;
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
                $"SELECT dmc.user_id, 0 as channel_id, dmc.count_date, SUM(dmc.message_count) AS message_count, du.id, du.username, du.timezone, du.last_online, du.archived, du.primary_channel_id, du.identity_user_id, du.birthdate FROM daily_message_count dmc INNER JOIN discord_user du ON dmc.user_id = du.id WHERE dmc.user_id = {userId} GROUP BY dmc.user_id, dmc.count_date, du.id, du.username, du.timezone, du.last_online, du.archived, du.primary_channel_id, du.identity_user_id, du.birthdate ORDER BY message_count DESC LIMIT {numOfDays}")
            .ToListAsync();
    }


    public async Task<IEnumerable<DailyMessageCountModel>> GetUsersTopDaysInChannel(ulong userId, ulong channelId, int numOfDays)
    {
        return await Context.DailyMessageCounts
            .FromSqlRaw(
                $"SELECT dmc.user_id, dmc.channel_id, dmc.count_date, SUM(dmc.message_count) AS message_count, du.id, du.username, du.timezone, du.last_online, du.archived, du.primary_channel_id, du.identity_user_id, du.birthdate FROM daily_message_count dmc INNER JOIN discord_user du ON dmc.user_id = du.id WHERE dmc.user_id = {userId} AND dmc.channel_id = {channelId} GROUP BY dmc.user_id, dmc.channel_id, dmc.count_date, du.id, du.username, du.timezone, du.last_online, du.archived, du.primary_channel_id, du.identity_user_id, du.birthdate ORDER BY message_count DESC LIMIT {numOfDays}")
            .ToListAsync();
    }
}