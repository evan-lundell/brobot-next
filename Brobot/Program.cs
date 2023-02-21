using Brobot.Contexts;
using Brobot.Repositories;
using Brobot.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Brobot;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        CreateServices(builder);
        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        var client = app.Services.GetRequiredService<DiscordSocketClient>();
        if (string.IsNullOrWhiteSpace(app.Configuration["BrobotToken"]))
        {
            Console.WriteLine("No token provided");
            return;
        }

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

    private static void CreateServices(WebApplicationBuilder builder)
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
    }
}




