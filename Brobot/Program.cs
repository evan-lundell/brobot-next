using Brobot.Contexts;
using Brobot.Repositories;
using Brobot.Services;
using Discord.Interactions;
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
            await client.LoginAsync(Discord.TokenType.Bot, app.Configuration["BrobotToken"] ?? "");
            await client.StartAsync();
        }

        var eventHandler = app.Services.GetRequiredService<DiscordEventHandler>();
        await eventHandler.StartAsync();
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
            GatewayIntents = Discord.GatewayIntents.All
        }));
        builder.Services.AddSingleton<InteractionService>();
        builder.Services.AddSingleton<DiscordEventHandler>();
        builder.Services.AddDbContext<BrobotDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
        });
        builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
    }
}




