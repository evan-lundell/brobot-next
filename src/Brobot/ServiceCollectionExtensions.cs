using System.Text;
using Brobot.Contexts;
using Brobot.HostedServices;
using Brobot.Configuration;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.TaskQueue;
using Brobot.Workers;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        services.AddScoped<IVersionService, VersionService>();
        services.AddScoped<IAssemblyService, AssemblyService>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueuedHostedService>();
        return services;
    }

    public static IServiceCollection AddUserManagement(this IServiceCollection services, IConfiguration config)
    {
        var jwtOptions = config.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

        services.AddIdentity<ApplicationUserModel, IdentityRole>()
            .AddEntityFrameworkStores<BrobotDbContext>()
            .AddDefaultTokenProviders();

        services.AddSingleton<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidAudience = jwtOptions.ValidAudience,
                    ValidIssuer = jwtOptions.ValidIssuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
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
        var options = config.GetSection(JobsOptions.SectionName).Get<JobsOptions>() ?? new JobsOptions();
        services.AddCronJob<ReminderWorker>(o => o.CronExpression = options.ReminderCron);
        services.AddCronJob<BirthdayWorker>(o => o.CronExpression = options.BirthdayCron);
        services.AddCronJob<HotOpWorker>(o => o.CronExpression = options.HotOpCron);
        services.AddCronJob<MonthlyStatsWorker>(o => o.CronExpression = options.MonthlyStatsCron);
        return services;
    }
    
    public static IServiceCollection AddBrobotServices(this IServiceCollection services, IConfiguration config)
    {
        var options = config.GetSection(ExternalApisOptions.SectionName).Get<ExternalApisOptions>()!;
        services.AddSingleton<ISyncService, SyncService>();
        services.AddScoped<IHotOpService, HotOpService>();
        services.AddScoped<IScheduledMessageService, ScheduledMessageService>();
        services.AddScoped<IMessageCountService, MessageCountService>();
        services.AddScoped<ISecretSantaService, SecretSantaService>();
        services.AddSingleton<IStopWordService, StopWordService>();
        services.AddSingleton<IWordCountService, WordCountService>();
        services.AddHttpClient<IGiphyService, GiphyService>(c =>
        {
            c.BaseAddress = new Uri(options.GiphyBaseUrl);
        });
        services.AddHttpClient<IRandomFactService, RandomFactService>(c =>
        {
            c.BaseAddress = new Uri(options.RandomFactBaseUrl);
        });
        services.AddHttpClient<IDictionaryService, DictionaryService>(c =>
        {
            c.BaseAddress = new Uri(options.DictionaryBaseUrl);
        });
        services.AddHttpClient<IWordCloudService, WordCloudService>(c =>
        {
            c.BaseAddress = new Uri(options.QuickChartBaseUrl);
        });
        services.AddScoped<IStatsService, StatsService>();

        return services;
    }
    
    public static IServiceCollection AddBrobotOptions(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<JobsOptions>()
            .Bind(config.GetSection(JobsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<ExternalApisOptions>()
            .Bind(config.GetSection("ExternalApis"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<JwtOptions>()
            .Bind(config.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
            
        services.AddOptions<DiscordOptions>()
            .Bind(config.GetSection(DiscordOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<GeneralOptions>()
            .Bind(config.GetSection(GeneralOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        return services;
    }
}