using System.Collections;
using AutoMapper;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared.Responses;
using TimeZoneConverter;

namespace Brobot.Services;

public class MessageCountService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public MessageCountService(IUnitOfWork uow, ILogger<MessageCountService> logger, IMapper mapper)
    {
        _uow = uow;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DailyMessageCountResponse>> GetUsersDailyMessageCountForChannel(ulong userId, ulong channelId, int numOfDays)
    {
        var userModel = await _uow.Users.GetByIdNoTracking(userId);
        if (userModel == null || string.IsNullOrWhiteSpace(userModel.Timezone))
        {
            return Array.Empty<DailyMessageCountResponse>();
        }
        
        var (currentDate, startDate) = GetDates(numOfDays, userModel.Timezone);
        var counts = await _uow.DailyMessageCounts.Find((dmc) =>
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
        var counts = await _uow.DailyMessageCounts.Find((dmc) =>
            dmc.UserId == userModel.Id && dmc.CountDate >= startDate && dmc.CountDate <= currentDate);
        return GetDailyMessageCountResponses(counts, userModel, startDate, currentDate);
    }

    public async Task AddToDailyCount(ulong userId, ulong channelId, DateOnly? countDate = null)
    {
        UserModel? user = null;
        if (countDate == null)
        {
            user = await _uow.Users.GetById(userId);
            if (string.IsNullOrWhiteSpace(user?.Timezone))
            {
                return;
            }

            var timezone = TZConvert.GetTimeZoneInfo(user.Timezone);
            countDate = DateOnly.FromDateTime(DateTime.UtcNow + timezone.GetUtcOffset(DateTime.UtcNow));
        }
        var dailyMessageCount = (await _uow.DailyMessageCounts
                .Find((dmc) => dmc.UserId == userId && dmc.ChannelId == channelId && dmc.CountDate == countDate))
            .FirstOrDefault();
        
        if (dailyMessageCount == null)
        {
            user ??= await _uow.Users.GetById(userId);

            var channel = await _uow.Channels.GetById(channelId);
            if (user == null || channel == null)
            {
                return;
            }

            await _uow.DailyMessageCounts.Add(new DailyMessageCountModel
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

        await _uow.CompleteAsync();
    }

    public async Task<IEnumerable<DailyMessageCountResponse>> GetUsersTopDays(UserModel userModel, int numOfDays)
    {
        if (string.IsNullOrWhiteSpace(userModel.Timezone))
        {
            return Array.Empty<DailyMessageCountResponse>();
        }

        var counts = await _uow.DailyMessageCounts.GetUsersTopDays(userModel.Id, numOfDays);
        var userResponse = _mapper.Map<UserResponse>(userModel);
        return counts.Select((c) => new DailyMessageCountResponse
        {
            User = userResponse,
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

        var userResponse = _mapper.Map<UserResponse>(userModel);
        var counts = await _uow.DailyMessageCounts.GetUsersTopDaysInChannel(userModel.Id, channelId, numOfDays);
        return counts.Select((c) => new DailyMessageCountResponse
        {
            User = userResponse,
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
        var counts = await _uow.DailyMessageCounts.GetTopForDate(DateOnly.FromDateTime(userNow.DateTime));
        return counts.Select((c) => new DailyMessageCountResponse
        {
            CountDate = c.CountDate,
            MessageCount = c.MessageCount,
            User = _mapper.Map<UserResponse>(c.User)
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
        var counts = await _uow.DailyMessageCounts.GetTopForDateByChannel(DateOnly.FromDateTime(userNow.DateTime), channelId);
        return counts.Select((c) => new DailyMessageCountResponse
        {
            CountDate = c.CountDate,
            MessageCount = c.MessageCount,
            User = _mapper.Map<UserResponse>(c.User)
        });
    }

    public async Task<IEnumerable<DailyMessageCountResponse>> GetTotalDailyMessageCounts(int numOfDays, string? usersTimezone)
    {
        if (string.IsNullOrWhiteSpace(usersTimezone))
        {
            usersTimezone = "UTC";
        }

        var (currentDate, startDate) = GetDates(numOfDays, usersTimezone);
        var counts = await _uow.DailyMessageCounts.GetTotalDailyMessageCounts(startDate, currentDate);
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
        var counts = await _uow.DailyMessageCounts.GetTotalDailyMessageCountsByChannel(startDate, currentDate, channelId);
        var fakeUser = new UserModel
        {
            Username = ""
        };
        return GetDailyMessageCountResponses(counts, fakeUser, startDate, currentDate);
    }
    
    public async Task<IEnumerable<DailyMessageCountResponse>> GetTotalTopDays(int numOfDays)
    {
        var counts = await _uow.DailyMessageCounts.GetTotalTopDays(numOfDays);
        var fakeUser = new UserResponse
        {
            Username = ""
        };
        return counts.Select((dmc) => new DailyMessageCountResponse
        {
            CountDate = dmc.CountDate,
            MessageCount = dmc.MessageCount,
            User = fakeUser
        });
    }
    
    public async Task<IEnumerable<DailyMessageCountResponse>> GetTotalTopDaysByChannel(ulong channelId, int numOfDays)
    {
        var counts = await _uow.DailyMessageCounts.GetTotalTopDaysByChannel(channelId, numOfDays);
        var fakeUser = new UserResponse
        {
            Username = ""
        };
        return counts.Select((dmc) => new DailyMessageCountResponse
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
        var userResponse = _mapper.Map<UserResponse>(userModel);
        var countsResponse = counts
            .GroupBy((c) => c.CountDate)
            .Select((x) => new DailyMessageCountResponse
            {
                CountDate = x.First().CountDate,
                MessageCount = x.Sum((mc) => mc.MessageCount),
                User = userResponse
            })
            .ToDictionary((c) => c.CountDate);
            
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