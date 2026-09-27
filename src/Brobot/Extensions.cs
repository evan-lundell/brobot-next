using Brobot.Workers;
using Discord;

namespace Brobot;

public static class Extensions
{
    public static void AddIfNotNull<T>(this List<T> list, params T?[] range)
    {
        list.AddRange(range.Where(e => e != null)!);
    }

    public static IServiceCollection AddCronJob<T>(this IServiceCollection services, Action<ICronWorkerConfig<T>> options) where T : CronWorkerBase
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options), "Please provide config.");
        }

        var config = new CronWorkerConfig<T>();
        options.Invoke(config);
        if (string.IsNullOrWhiteSpace(config.CronExpression))
        {
            throw new ArgumentNullException(nameof(config.CronExpression), "Please provide cron expression");
        }

        services.AddSingleton<ICronWorkerConfig<T>>(config);
        services.AddHostedService<T>();
        return services;
    }


    extension(DateTimeOffset dateTimeOffset)
    {
        public DateTimeOffset AdjustToUtc(string timezoneName)
        {
           var local = new DateTime(dateTimeOffset.Ticks, DateTimeKind.Unspecified);
            var timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneName);
            return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, timezone), TimeSpan.Zero);
        }

        public DateTimeOffset AdjustToUsersTimezone(string timezoneName)
        {
            var timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneName);
            return TimeZoneInfo.ConvertTime(dateTimeOffset, timezone);
        }
    }
    
    extension(IVoiceChannel voiceChannel)
    {
        public async Task<IEnumerable<IGuildUser>> GetConnectedUsersAsync()
        {
            var users = await voiceChannel.GetUsersAsync().FlattenAsync();
            return users.Where(u => u.VoiceChannel?.Id == voiceChannel.Id);
        }
    }
}