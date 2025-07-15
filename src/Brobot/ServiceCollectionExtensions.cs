using System.Text;
using Brobot.Contexts;
using Brobot.HostedServices;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.Workers;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Brobot;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBrobotInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<BrobotDbContext>(db =>
        {
            db.UseNpgsql(config.GetConnectionString("Default"), npgsql =>
            {
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "brobot");
            });
        });
        services.AddTransient<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<Random>();
        return services;
    }

    public static IServiceCollection AddUserManagement(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<UsersDbContext>(db =>
        {
            db.UseNpgsql(config.GetConnectionString("Default"), npgsql =>
            {
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "auth");
            });
        });

        services.AddIdentityCore<IdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddDefaultTokenProviders();

        services.AddSingleton<IJwtService, JwtService>();
        services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidAudience = config["ValidAudience"] ?? "",
                    ValidIssuer = config["ValidIssuer"] ?? "",
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["JwtSigningKey"] ?? ""))
                };
            });

        return services;
    }
    
    public static IServiceCollection AddDiscord(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IDiscordClient>(new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All,
            MessageCacheSize = 100
        }));

        services.AddHttpClient<DiscordOauthService>();
        services.AddHostedService<DiscordBotHostedService>();
        return services;
    }
    
    public static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration config)
    {
        services.AddCronJob<ReminderWorker>(o => o.CronExpression = "* * * * *");
        services.AddCronJob<BirthdayWorker>(o => o.CronExpression = "0 12 * * *");
        services.AddCronJob<HotOpWorker>(o => o.CronExpression = "* * * * *");
        services.AddCronJob<MonthlyStatsWorker>(o => o.CronExpression = "0 12 1 * *");
        return services;
    }
    
    public static IServiceCollection AddBrobotServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<ISyncService, SyncService>();
        services.AddScoped<IHotOpService, HotOpService>();
        services.AddScoped<IScheduledMessageService, ScheduledMessageService>();
        services.AddScoped<IMessageCountService, MessageCountService>();
        services.AddScoped<SecretSantaService>();
        services.AddSingleton<IStopWordService, StopWordService>();
        services.AddSingleton<WordCountService>();
        services.AddHttpClient<IGiphyService, GiphyService>(c =>
        {
            c.BaseAddress = new Uri(config["GiphyBaseUrl"] ?? "");
        });
        services.AddHttpClient<IRandomFactService, RandomFactService>(c =>
        {
            c.BaseAddress = new Uri(config["RandomFactBaseUrl"] ?? "");
        });
        services.AddHttpClient<IDictionaryService, DictionaryService>(c =>
        {
            c.BaseAddress = new Uri(config["DictionaryBaseUrl"] ?? "");
        });
        services.AddHttpClient<IWordCloudService, WordCloudService>(c =>
        {
            c.BaseAddress = new Uri(config["QuickChartBaseUrl"] ?? "https://quickchart.io");
        });

        return services;
    }
}