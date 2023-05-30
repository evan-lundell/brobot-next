using Brobot.Middlewares;
using Brobot.Workers;

public static class Extensions
{
    public static void AddIfNotNull<T>(this List<T> list, T? value)
    {
        if (value != null)
        {
            list.Add(value);
        }
    }

    public static void AddIfNotNull<T>(this List<T> list, params T?[] range)
    {
        foreach (var element in range)
        {
            list.AddIfNotNull(element);
        }
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

    public static IApplicationBuilder UseDiscordUser(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DiscordUserMiddleware>();
    }
}