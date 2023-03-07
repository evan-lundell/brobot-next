using Brobot.Contexts;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.Workers;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Victoria.Node;

namespace Brobot;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        CreateServices(builder, args);
        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        if (app.Environment.IsProduction())
        {
            var context = app.Services.GetRequiredService<BrobotDbContext>();
            await context.Database.MigrateAsync();
        }

        var client = app.Services.GetRequiredService<DiscordSocketClient>();
        if (string.IsNullOrWhiteSpace(app.Configuration["BrobotToken"]))
        {
            Console.WriteLine("No token provided");
            return;
        }

        app.Configuration["NoSync"] = args.Contains("--no-sync").ToString();

        if (!args.Contains("--no-bot"))
        {
            var eventHandler = app.Services.GetRequiredService<DiscordEventHandler>();
            eventHandler.Start();
            await client.LoginAsync(Discord.TokenType.Bot, app.Configuration["BrobotToken"] ?? "");
            await client.StartAsync();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

    private static void CreateServices(WebApplicationBuilder builder, string[] args)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = Discord.GatewayIntents.All,
            MessageCacheSize = 100
        }));
        builder.Services.AddDbContext<BrobotDbContext>(
            options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
        );
        builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
        builder.Services.AddSingleton<Random>();
        builder.Services.AddHttpClient<IGiphyService, GiphyService>((configure) =>
        {
            configure.BaseAddress = new Uri(builder.Configuration["GiphyBaseUrl"] ?? "");
        });
        builder.Services.AddHttpClient<IRandomFactService, RandomFactService>((configure) =>
        {
            configure.BaseAddress = new Uri(builder.Configuration["RandomFactBaseUrl"] ?? "");
        });
        builder.Services.AddHttpClient<IDictionaryService, DictionaryService>((configure) =>
        {
            configure.BaseAddress = new Uri(builder.Configuration["DictionaryBaseUrl"] ?? "");
        });
        builder.Services.AddSingleton<DiscordEventHandler>();
        builder.Services.AddSingleton<ISyncService, SyncService>();
        builder.Services.AddSingleton<HotOpService>();

        if (!args.Contains("--no-jobs"))
        {
            builder.Services.AddCronJob<ReminderWorker>((options) =>
            {
                options.CronExpression = "* * * * *";
            });
            builder.Services.AddCronJob<BirthdayWorker>((options) =>
            {
                options.CronExpression = "0 12 * * *";
            });
        }

        builder.Services.AddLogging();
        var nodeConfig = new NodeConfiguration
        {
            Authorization = builder.Configuration["LavalinkPassword"],
            Hostname = builder.Configuration["LavalinkHost"]
        };
        builder.Services.AddSingleton<NodeConfiguration>(nodeConfig);
        builder.Services.AddSingleton<LavaNode>();
    }
}
