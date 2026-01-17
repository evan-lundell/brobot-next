using Brobot.Mappers;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared.Responses;
using TimeZoneConverter;

namespace Brobot.Services;

public class MessageCountService(IUnitOfWork uow, ILogger<MessageCountService> logger) : IMessageCountService
{
    public async Task<IEnumerable<DailyMessageCountResponse>> GetUsersDailyMessageCountForChannel(ulong userId, ulong channelId, int numOfDays)
    {
        logger.LogInformation("Getting daily message count for channel {ChannelId} for user {UserId}", channelId, userId);
        var userModel = await uow.Users.GetByIdNoTracking(userId);
        if (userModel == null || string.IsNullOrWhiteSpace(userModel.Timezone))
        {
            logger.LogWarning("User {UserId} not found or has no time zone", userId);
            return [];
        }
        
        var (currentDate, startDate) = GetDates(numOfDays, userModel.Timezone);
        var counts = await uow.DailyMessageCounts.Find(dmc =>
            dmc.DiscordUserId == userId && dmc.ChannelId == channelId && dmc.CountDate >= startDate && dmc.CountDate <= currentDate);
        
        logger.LogInformation("Finished getting daily message count for channel {ChannelId} for user {UserId}", channelId, userId);
        return GetDailyMessageCountResponses(counts, userModel, startDate, currentDate);
    }

    public async Task<IEnumerable<DailyMessageCountResponse>> GetUsersTotalDailyMessageCounts(DiscordUserModel discordUserModel, int numOfDays)
    {
        logger.LogInformation("Getting daily message count for user {UserId}", discordUserModel.Id);
        if (string.IsNullOrWhiteSpace(discordUserModel.Timezone))
        {
            logger.LogWarning("User {UserId} not found or has no time zone", discordUserModel.Id);
            return [];
        }

        var (currentDate, startDate) = GetDates(numOfDays, discordUserModel.Timezone);
        var counts = await uow.DailyMessageCounts.Find(dmc =>
            dmc.DiscordUserId == discordUserModel.Id && dmc.CountDate >= startDate && dmc.CountDate <= currentDate);
        
        logger.LogInformation("Finished getting daily message count for user {UserId}", discordUserModel.Id);
        return GetDailyMessageCountResponses(counts, discordUserModel, startDate, currentDate);
    }

    public async Task AddToDailyCount(ulong userId, ulong channelId, DateOnly? countDate = null)
    {
        try
        {
            DiscordUserModel? user = null;
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
                    .Find(dmc => dmc.DiscordUserId == userId && dmc.ChannelId == channelId && dmc.CountDate == countDate))
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
                    DiscordUser = user,
                    DiscordUserId = userId,
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding daily count for user {UserId}", userId);
        }
    }

    public async Task<IEnumerable<DailyMessageCountResponse>> GetUsersTopDays(DiscordUserModel discordUserModel, int numOfDays)
    {
        logger.LogInformation("Getting top days for user {UserId}", discordUserModel.Id);
        if (string.IsNullOrWhiteSpace(discordUserModel.Timezone))
        {
            logger.LogWarning("User {UserId} has no time zone", discordUserModel.Id);
            return [];
        }

        var counts = await uow.DailyMessageCounts.GetUsersTopDays(discordUserModel.Id, numOfDays);
        logger.LogInformation("Finished getting top days for user {UserId}", discordUserModel.Id);
        return counts.Select(c => new DailyMessageCountResponse
        {
            DiscordUser = discordUserModel.ToUserResponse(),
            CountDate = c.CountDate,
            MessageCount = c.MessageCount
        });
    }

    public async Task<IEnumerable<DailyMessageCountResponse>> GetUsersTopDaysByChannel(DiscordUserModel discordUserModel, ulong channelId, int numOfDays)
    {
        logger.LogInformation("Getting top days for user {UserId} in channel {ChannelId}", discordUserModel.Id, channelId);
        if (string.IsNullOrWhiteSpace(discordUserModel.Timezone))
        {
            return [];
        }

        var counts = await uow.DailyMessageCounts.GetUsersTopDaysInChannel(discordUserModel.Id, channelId, numOfDays);
        logger.LogInformation("Finished getting top days for user {UserId} in channel {ChannelId}", discordUserModel.Id, channelId);
        return counts.Select(c => new DailyMessageCountResponse
        {
            DiscordUser = discordUserModel.ToUserResponse(),
            CountDate = c.CountDate,
            MessageCount = c.MessageCount
        });
    }
    
    public async Task<IEnumerable<DailyMessageCountResponse>> GetTopToday(DiscordUserModel discordUserModel)
    {
        logger.LogInformation("Getting top messages today for user {UserId}", discordUserModel.Id);
        if (string.IsNullOrWhiteSpace(discordUserModel.Timezone))
        {
            logger.LogWarning("User {UserId} has no time zone", discordUserModel.Id);
            return [];
        }
        
        var now = DateTimeOffset.UtcNow;
        var userNow = now.AdjustToUsersTimezone(discordUserModel.Timezone);
        var counts = await uow.DailyMessageCounts.GetTopForDate(DateOnly.FromDateTime(userNow.DateTime));
        logger.LogInformation("Finished getting top messages today for user {UserId}", discordUserModel.Id);
        return counts.Select(c => new DailyMessageCountResponse
        {
            CountDate = c.CountDate,
            MessageCount = c.MessageCount,
            DiscordUser = c.DiscordUser.ToUserResponse()
        });
    }

    public async Task<IEnumerable<DailyMessageCountResponse>> GetTopTodayByChannel(DiscordUserModel discordUserModel, ulong channelId)
    {
        logger.LogInformation("Getting top messages today for user {UserId} and channel {ChannelId}", discordUserModel.Id, channelId);
        if (string.IsNullOrWhiteSpace(discordUserModel.Timezone))
        {
            logger.LogWarning("User {UserId} has no time zone", discordUserModel.Id);
            return [];
        }
        
        var now = DateTimeOffset.UtcNow;
        var userNow = now.AdjustToUsersTimezone(discordUserModel.Timezone);
        var counts = await uow.DailyMessageCounts.GetTopForDateByChannel(DateOnly.FromDateTime(userNow.DateTime), channelId);
        logger.LogInformation("Finished getting top messages today for user {UserId} and channel {ChannelId}", discordUserModel.Id, channelId);
        return counts.Select(c => new DailyMessageCountResponse
        {
            CountDate = c.CountDate,
            MessageCount = c.MessageCount,
            DiscordUser = c.DiscordUser.ToUserResponse()
        });
    }

    public async Task<IEnumerable<DailyMessageCountResponse>> GetTotalDailyMessageCounts(int numOfDays, string? usersTimezone)
    {
        logger.LogInformation("Getting total daily message counts");
        if (string.IsNullOrWhiteSpace(usersTimezone))
        {
            logger.LogWarning("No time zone");
            return [];
        }

        var (currentDate, startDate) = GetDates(numOfDays, usersTimezone);
        var counts = await uow.DailyMessageCounts.GetTotalDailyMessageCounts(startDate, currentDate);
        var fakeUser = new DiscordUserModel
        {
            Username = ""
        };
        logger.LogInformation("Finished getting total daily message counts");
        return GetDailyMessageCountResponses(counts, fakeUser, startDate, currentDate);
    }
    
    public async Task<IEnumerable<DailyMessageCountResponse>> GetTotalDailyMessageCountsByChannel(int numOfDays, ulong channelId, string? usersTimezone)
    {
        logger.LogInformation("Getting total daily message counts for channel {ChannelId}", channelId);
        if (string.IsNullOrWhiteSpace(usersTimezone))
        {
            logger.LogWarning("No time zone");
            return [];
        }

        var (currentDate, startDate) = GetDates(numOfDays, usersTimezone);
        var counts = await uow.DailyMessageCounts.GetTotalDailyMessageCountsByChannel(startDate, currentDate, channelId);
        var fakeUser = new DiscordUserModel
        {
            Username = ""
        };
        logger.LogInformation("Finished getting total daily message counts for channel {ChannelId}", channelId);
        return GetDailyMessageCountResponses(counts, fakeUser, startDate, currentDate);
    }
    
    public async Task<IEnumerable<DailyMessageCountResponse>> GetTotalTopDays(int numOfDays)
    {
        logger.LogInformation("Getting total top days");
        var counts = await uow.DailyMessageCounts.GetTotalTopDays(numOfDays);
        var fakeUser = new DiscordUserResponse
        {
            Username = ""
        };
        logger.LogInformation("Finished getting total top days");
        return counts.Select(dmc => new DailyMessageCountResponse
        {
            CountDate = dmc.CountDate,
            MessageCount = dmc.MessageCount,
            DiscordUser = fakeUser
        });
    }
    
    public async Task<IEnumerable<DailyMessageCountResponse>> GetTotalTopDaysByChannel(ulong channelId, int numOfDays)
    {
        logger.LogInformation("Getting total top days for channel {ChannelId}", channelId);
        var counts = await uow.DailyMessageCounts.GetTotalTopDaysByChannel(channelId, numOfDays);
        var fakeUser = new DiscordUserResponse
        {
            Username = ""
        };
        logger.LogInformation("Finished getting total top days for channel {ChannelId}", channelId);
        return counts.Select(dmc => new DailyMessageCountResponse
        {
            CountDate = dmc.CountDate,
            MessageCount = dmc.MessageCount,
            DiscordUser = fakeUser
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

    private List<DailyMessageCountResponse> GetDailyMessageCountResponses(IEnumerable<DailyMessageCountModel> counts, DiscordUserModel discordUserModel, DateOnly startDate, DateOnly currentDate)
    {
        var userResponse = discordUserModel.ToUserResponse();
        var countsResponse = counts
            .GroupBy(c => c.CountDate)
            .Select(x => new DailyMessageCountResponse
            {
                CountDate = x.First().CountDate,
                MessageCount = x.Sum(mc => mc.MessageCount),
                DiscordUser = userResponse
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
                    DiscordUser = userResponse,
                    CountDate = nextDate,
                    MessageCount = 0
                });
            }
            nextDate = nextDate.AddDays(1);
        }

        return countsWithEmptyDays;
    }
}