using Brobot.Mappers;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared.Responses;
using TimeZoneConverter;

namespace Brobot.Services;

public class MessageCountService(IUnitOfWork uow)
{
    public async Task<IEnumerable<DailyMessageCountResponse>> GetUsersDailyMessageCountForChannel(ulong userId, ulong channelId, int numOfDays)
    {
        var userModel = await uow.Users.GetByIdNoTracking(userId);
        if (userModel == null || string.IsNullOrWhiteSpace(userModel.Timezone))
        {
            return Array.Empty<DailyMessageCountResponse>();
        }
        
        var (currentDate, startDate) = GetDates(numOfDays, userModel.Timezone);
        var counts = await uow.DailyMessageCounts.Find(dmc =>
            dmc.UserId == userId && dmc.ChannelId == channelId && dmc.CountDate >= startDate && dmc.CountDate <= currentDate);
        return GetDailyMessageCountResponses(counts, userModel, startDate, currentDate);
    }

    public async Task<IEnumerable<DailyMessageCountResponse>> GetUsersTotalDailyMessageCounts(UserModel userModel, int numOfDays)
    {
        if (string.IsNullOrWhiteSpace(userModel.Timezone))
        {
            return Array.Empty<DailyMessageCountResponse>();
        }

        var (currentDate, startDate) = GetDates(numOfDays, userModel.Timezone);
        var counts = await uow.DailyMessageCounts.Find(dmc =>
            dmc.UserId == userModel.Id && dmc.CountDate >= startDate && dmc.CountDate <= currentDate);
        return GetDailyMessageCountResponses(counts, userModel, startDate, currentDate);
    }

    public async Task AddToDailyCount(ulong userId, ulong channelId, DateOnly? countDate = null)
    {
        UserModel? user = null;
        if (countDate == null)
        {
            user = await uow.Users.GetById(userId);
            if (string.IsNullOrWhiteSpace(user?.Timezone))
            {
                return;
            }

            var timezone = TZConvert.GetTimeZoneInfo(user.Timezone);
            countDate = DateOnly.FromDateTime(DateTime.UtcNow + timezone.GetUtcOffset(DateTime.UtcNow));
        }
        var dailyMessageCount = (await uow.DailyMessageCounts
                .Find(dmc => dmc.UserId == userId && dmc.ChannelId == channelId && dmc.CountDate == countDate))
            .FirstOrDefault();
        
        if (dailyMessageCount == null)
        {
            user ??= await uow.Users.GetById(userId);

            var channel = await uow.Channels.GetById(channelId);
            if (user == null || channel == null)
            {
                return;
            }

            await uow.DailyMessageCounts.Add(new DailyMessageCountModel
            {
                User = user,
                UserId = userId,
                Channel = channel,
                ChannelId = channelId,
                CountDate = countDate.Value,
                MessageCount = 1
            });
        }
        else
        {
            dailyMessageCount.MessageCount += 1;
        }

        await uow.CompleteAsync();
    }

    public async Task<IEnumerable<DailyMessageCountResponse>> GetUsersTopDays(UserModel userModel, int numOfDays)
    {
        if (string.IsNullOrWhiteSpace(userModel.Timezone))
        {
            return Array.Empty<DailyMessageCountResponse>();
        }

        var counts = await uow.DailyMessageCounts.GetUsersTopDays(userModel.Id, numOfDays);
        return counts.Select(c => new DailyMessageCountResponse
        {
            User = userModel.ToUserResponse(),
            CountDate = c.CountDate,
            MessageCount = c.MessageCount
        });
    }

    public async Task<IEnumerable<DailyMessageCountResponse>> GetUsersTopDaysByChannel(UserModel userModel, ulong channelId, int numOfDays)
    {
        if (string.IsNullOrWhiteSpace(userModel.Timezone))
        {
            return Array.Empty<DailyMessageCountResponse>();
        }

        var counts = await uow.DailyMessageCounts.GetUsersTopDaysInChannel(userModel.Id, channelId, numOfDays);
        return counts.Select(c => new DailyMessageCountResponse
        {
            User = userModel.ToUserResponse(),
            CountDate = c.CountDate,
            MessageCount = c.MessageCount
        });
    }
    
    public async Task<IEnumerable<DailyMessageCountResponse>> GetTopToday(UserModel userModel)
    {
        if (string.IsNullOrWhiteSpace(userModel.Timezone))
        {
            return Array.Empty<DailyMessageCountResponse>();
        }
        
        var now = DateTimeOffset.UtcNow;
        var userNow = now.AdjustToUsersTimezone(userModel.Timezone);
        var counts = await uow.DailyMessageCounts.GetTopForDate(DateOnly.FromDateTime(userNow.DateTime));
        return counts.Select(c => new DailyMessageCountResponse
        {
            CountDate = c.CountDate,
            MessageCount = c.MessageCount,
            User = userModel.ToUserResponse()
        });
    }

    public async Task<IEnumerable<DailyMessageCountResponse>> GetTopTodayByChannel(UserModel userModel, ulong channelId)
    {
        if (string.IsNullOrWhiteSpace(userModel.Timezone))
        {
            return Array.Empty<DailyMessageCountResponse>();
        }
        
        var now = DateTimeOffset.UtcNow;
        var userNow = now.AdjustToUsersTimezone(userModel.Timezone);
        var counts = await uow.DailyMessageCounts.GetTopForDateByChannel(DateOnly.FromDateTime(userNow.DateTime), channelId);
        return counts.Select(c => new DailyMessageCountResponse
        {
            CountDate = c.CountDate,
            MessageCount = c.MessageCount,
            User = userModel.ToUserResponse()
        });
    }

    public async Task<IEnumerable<DailyMessageCountResponse>> GetTotalDailyMessageCounts(int numOfDays, string? usersTimezone)
    {
        if (string.IsNullOrWhiteSpace(usersTimezone))
        {
            usersTimezone = "UTC";
        }

        var (currentDate, startDate) = GetDates(numOfDays, usersTimezone);
        var counts = await uow.DailyMessageCounts.GetTotalDailyMessageCounts(startDate, currentDate);
        var fakeUser = new UserModel
        {
            Username = ""
        };
        return GetDailyMessageCountResponses(counts, fakeUser, startDate, currentDate);
    }
    
    public async Task<IEnumerable<DailyMessageCountResponse>> GetTotalDailyMessageCountsByChannel(int numOfDays, ulong channelId, string? usersTimezone)
    {
        if (string.IsNullOrWhiteSpace(usersTimezone))
        {
            usersTimezone = "UTC";
        }

        var (currentDate, startDate) = GetDates(numOfDays, usersTimezone);
        var counts = await uow.DailyMessageCounts.GetTotalDailyMessageCountsByChannel(startDate, currentDate, channelId);
        var fakeUser = new UserModel
        {
            Username = ""
        };
        return GetDailyMessageCountResponses(counts, fakeUser, startDate, currentDate);
    }
    
    public async Task<IEnumerable<DailyMessageCountResponse>> GetTotalTopDays(int numOfDays)
    {
        var counts = await uow.DailyMessageCounts.GetTotalTopDays(numOfDays);
        var fakeUser = new UserResponse
        {
            Username = ""
        };
        return counts.Select(dmc => new DailyMessageCountResponse
        {
            CountDate = dmc.CountDate,
            MessageCount = dmc.MessageCount,
            User = fakeUser
        });
    }
    
    public async Task<IEnumerable<DailyMessageCountResponse>> GetTotalTopDaysByChannel(ulong channelId, int numOfDays)
    {
        var counts = await uow.DailyMessageCounts.GetTotalTopDaysByChannel(channelId, numOfDays);
        var fakeUser = new UserResponse
        {
            Username = ""
        };
        return counts.Select(dmc => new DailyMessageCountResponse
        {
            CountDate = dmc.CountDate,
            MessageCount = dmc.MessageCount,
            User = fakeUser
        });
    }

    private (DateOnly CurrentDate, DateOnly StartDate) GetDates(int numOfDays, string timezoneString)
    {
        var timezone = TZConvert.GetTimeZoneInfo(timezoneString);
        var userDateTimeNow = DateTime.UtcNow + timezone.GetUtcOffset(DateTime.UtcNow);
        var currentDate = DateOnly.FromDateTime(userDateTimeNow);
        var startDate = currentDate.AddDays((numOfDays - 1) * -1);
        return (currentDate, startDate);
    }

    private List<DailyMessageCountResponse> GetDailyMessageCountResponses(IEnumerable<DailyMessageCountModel> counts, UserModel userModel, DateOnly startDate, DateOnly currentDate)
    {
        var userResponse = userModel.ToUserResponse();
        var countsResponse = counts
            .GroupBy(c => c.CountDate)
            .Select(x => new DailyMessageCountResponse
            {
                CountDate = x.First().CountDate,
                MessageCount = x.Sum(mc => mc.MessageCount),
                User = userResponse
            })
            .ToDictionary(c => c.CountDate);
            
        var countsWithEmptyDays = new List<DailyMessageCountResponse>();
        var nextDate = startDate;
        while (nextDate <= currentDate)
        {
            if (countsResponse.TryGetValue(nextDate, out var count))
            {
                countsWithEmptyDays.Add(count);
            }
            else
            {
                countsWithEmptyDays.Add(new DailyMessageCountResponse
                {
                    User = userResponse,
                    CountDate = nextDate,
                    MessageCount = 0
                });
            }
            nextDate = nextDate.AddDays(1);
        }

        return countsWithEmptyDays;
    }
}