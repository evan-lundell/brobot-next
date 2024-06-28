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
            .Where(dmc => dmc.UserId == userId)
            .GroupBy(d => new { d.UserId, d.CountDate })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.CountDate,
                MessageCount = g.Sum(d => d.MessageCount)
            })
            .OrderByDescending(r => r.MessageCount)
            .Take(numOfDays)
            .Join(Context.Users,
                dmc => dmc.UserId,
                user => user.Id,
                (dmc, user) => new DailyMessageCountModel
                {
                    User = user,
                    UserId = dmc.UserId,
                    CountDate = dmc.CountDate,
                    ChannelId = 0,
                    Channel = new ChannelModel
                    {
                        Id = 0,
                        Name = "Global",
                        GuildId = 0,
                        Guild = new GuildModel
                        {
                            Id = 0,
                            Name = "Global"
                        }
                    },
                    MessageCount = dmc.MessageCount

                })
            .ToListAsync();
    }


    public async Task<IEnumerable<DailyMessageCountModel>> GetUsersTopDaysInChannel(ulong userId, ulong channelId, int numOfDays)
    {
        return await Context.DailyMessageCounts
            .Where(dmc => dmc.UserId == userId && dmc.ChannelId == channelId)
            .GroupBy(d => new { d.UserId, d.ChannelId, d.CountDate })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.ChannelId,
                g.Key.CountDate,
                MessageCount = g.Sum(d => d.MessageCount)
            })
            .OrderByDescending(r => r.MessageCount)
            .Take(numOfDays)
            .Join(Context.Users,
                dmc => dmc.UserId,
                user => user.Id,
                (dmc, user) => new DailyMessageCountModel
                {
                    User = user,
                    UserId = dmc.UserId,
                    CountDate = dmc.CountDate,
                    ChannelId = dmc.ChannelId,
                    Channel = new ChannelModel
                    {
                        Id = 0,
                        Name = "Global",
                        GuildId = 0,
                        Guild = new GuildModel
                        {
                            Id = 0,
                            Name = "Global"
                        }
                    },
                    MessageCount = dmc.MessageCount

                })
            .ToListAsync();
    }

    public async Task<IEnumerable<DailyMessageCountModel>> GetTopForDate(DateOnly date)
    {
        var result = await Context.DailyMessageCounts
            .Where(d => d.CountDate == date)
            .GroupBy(d => new { d.UserId, d.CountDate })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.CountDate,
                MessageCount = g.Sum(d => d.MessageCount)
            })
            .OrderByDescending(d => d.MessageCount)
            .Join(Context.Users,
                dmc => dmc.UserId,
                user => user.Id,
                (dmc, user) => new DailyMessageCountModel
                {
                    User = user,
                    UserId = dmc.UserId,
                    CountDate = dmc.CountDate,
                    ChannelId = 0,
                    Channel = new ChannelModel
                    {
                        Id = 0,
                        Name = "Global",
                        GuildId = 0,
                        Guild = new GuildModel
                        {
                            Id = 0,
                            Name = "Global"
                        }
                    },
                    MessageCount = dmc.MessageCount
                })
            .ToListAsync();
        return result;
    }


    public async Task<IEnumerable<DailyMessageCountModel>> GetTopForDateByChannel(DateOnly date, ulong channelId)
    {
        var result = await Context.DailyMessageCounts
            .Where(d => d.CountDate == date && d.ChannelId == channelId)
            .GroupBy(d => new { d.UserId, d.ChannelId, d.CountDate })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.ChannelId,
                g.Key.CountDate,
                MessageCount = g.Sum(d => d.MessageCount)
            })
            .OrderByDescending(d => d.MessageCount)
            .Join(Context.Users,
                dmc => dmc.UserId,
                user => user.Id,
                (dmc, user) => new DailyMessageCountModel
                {
                    User = user,
                    UserId = dmc.UserId,
                    CountDate = dmc.CountDate,
                    ChannelId = dmc.ChannelId,
                    Channel = new ChannelModel
                    {
                        Id = 0,
                        Name = "Global",
                        GuildId = 0,
                        Guild = new GuildModel
                        {
                            Id = 0,
                            Name = "Global"
                        }
                    },
                    MessageCount = dmc.MessageCount
                })
            .ToListAsync();
        return result;
    }

    public async Task<IEnumerable<DailyMessageCountModel>> GetTotalDailyMessageCounts(DateOnly startDate, DateOnly endDate)
    {
        return await Context.DailyMessageCounts
            .Where(dmc => dmc.CountDate >= startDate && dmc.CountDate <= endDate)
            .GroupBy(d => new { d.UserId, d.CountDate })
            .Select(g => new
            { 
                g.Key.UserId,
                g.Key.CountDate,
                MessageCount = g.Sum(d => d.MessageCount)
            })
            .OrderByDescending(d => d.CountDate)
            .Join(Context.Users,
                dmc => dmc.UserId,
                user => user.Id,
                (dmc, user) => new DailyMessageCountModel
                {
                    User = user,
                    UserId = dmc.UserId,
                    CountDate = dmc.CountDate,
                    ChannelId = 0,
                    Channel = new ChannelModel
                    {
                        Id = 0,
                        Name = "Global",
                        GuildId = 0,
                        Guild = new GuildModel
                        {
                            Id = 0,
                            Name = "Global"
                        }
                    },
                    MessageCount = dmc.MessageCount
                })
            .ToListAsync();
    }
    
    public async Task<IEnumerable<DailyMessageCountModel>> GetTotalDailyMessageCountsByChannel(DateOnly startDate, DateOnly endDate, ulong channelId)
    {
        return await Context.DailyMessageCounts
            .Where(dmc => dmc.CountDate >= startDate && dmc.CountDate <= endDate && dmc.ChannelId == channelId)
            .GroupBy(d => new { d.UserId, d.ChannelId, d.CountDate })
            .Select(g => new
            { 
                g.Key.UserId,
                g.Key.ChannelId,
                g.Key.CountDate,
                MessageCount = g.Sum(d => d.MessageCount)
            })
            .OrderByDescending(d => d.CountDate)
            .Join(Context.Users,
                dmc => dmc.UserId,
                user => user.Id,
                (dmc, user) => new DailyMessageCountModel
                {
                    User = user,
                    UserId = dmc.UserId,
                    CountDate = dmc.CountDate,
                    ChannelId = dmc.ChannelId,
                    Channel = new ChannelModel
                    {
                        Id = 0,
                        Name = "Global",
                        GuildId = 0,
                        Guild = new GuildModel
                        {
                            Id = 0,
                            Name = "Global"
                        }
                    },
                    MessageCount = dmc.MessageCount
                })
            .ToListAsync();
    }

    public async Task<IEnumerable<DailyMessageCountModel>> GetTotalTopDays(int numOfDays)
    {
        return await Context.DailyMessageCounts
            .GroupBy(dmc => new { dmc.CountDate })
            .Select(g => new DailyMessageCountModel
            {
                UserId = 0,
                User = new UserModel
                {
                    Id = 0,
                    Username = "Global"
                },
                ChannelId = 0,
                Channel = new ChannelModel
                {
                    Id = 0,
                    Name = "Global",
                    GuildId = 0,
                    Guild = new GuildModel
                    {
                        Id = 0,
                        Name = "Global"
                    }
                },
                CountDate = g.Key.CountDate,
                MessageCount = g.Sum(d => d.MessageCount)
            })
            .OrderByDescending(d => d.MessageCount)
            .Take(numOfDays)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<DailyMessageCountModel>> GetTotalTopDaysByChannel(ulong channelId, int numOfDays)
    {
        return await Context.DailyMessageCounts
            .Where(dmc => dmc.ChannelId == channelId)
            .GroupBy(dmc => new { dmc.ChannelId, dmc.CountDate })
            .Select(g => new DailyMessageCountModel
            {
                UserId = 0,
                User = new UserModel
                {
                    Id = 0,
                    Username = "Global"
                },
                ChannelId = g.Key.ChannelId,
                Channel = new ChannelModel
                {
                    Id = 0,
                    Name = "Global",
                    GuildId = 0,
                    Guild = new GuildModel
                    {
                        Id = 0,
                        Name = "Global"
                    }
                },
                CountDate = g.Key.CountDate,
                MessageCount = g.Sum(d => d.MessageCount)
            })
            .OrderByDescending(d => d.MessageCount)
            .Take(numOfDays)
            .ToListAsync();
    }
}