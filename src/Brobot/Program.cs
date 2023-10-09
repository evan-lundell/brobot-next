using System.Text;
using Brobot.Contexts;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.Workers;
using Discord.WebSocket;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Victoria.Node;

namespace Brobot;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        CreateServices(builder, args);
        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseWebAssemblyDebugging();
            app.UseExceptionHandler("/error-development");
        }
        else
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }

        if (app.Environment.IsProduction())
        {
            var usersDbContext = app.Services.GetRequiredService<UsersDbContext>();
            await usersDbContext.Database.MigrateAsync();
            var brobotDbContext = app.Services.GetRequiredService<BrobotDbContext>();
            await brobotDbContext.Database.MigrateAsync();
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
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRazorPages();
        app.UseDiscordUser();
        app.MapControllers();
        app.MapFallbackToFile("index.html");
        // ReSharper disable once MethodHasAsyncOverload
        app.Run();
    }

    private static void CreateServices(WebApplicationBuilder builder, string[] args)
    {
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen((options) =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
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
            builder.Services.AddCronJob<HotOpWorker>((options) =>
            {
                options.CronExpression = "* * * * *";
            });
            builder.Services.AddCronJob<WordCloudWorker>((options) =>
            {
                options.CronExpression = "49 20 * * *";
            });
        }

        builder.Services.AddLogging();
        var nodeConfig = new NodeConfiguration
        {
            Authorization = builder.Configuration["LavalinkPassword"],
            Hostname = builder.Configuration["LavalinkHost"],
            Port = 2333
        };
        builder.Services.AddSingleton(nodeConfig);
        builder.Services.AddSingleton<LavaNode>();
        builder.Services.AddAuthentication()
        .AddJwtBearer((options) =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidAudience = builder.Configuration["ValidAudience"] ?? "",
                ValidIssuer = builder.Configuration["ValidIssuer"] ?? "",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSigningKey"] ?? ""))
            };
        });
        builder.Services.AddDbContext<UsersDbContext>((options) =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
        });
        builder.Services.AddIdentityCore<IdentityUser>((options) =>
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
        builder.Services.AddSingleton<JwtService>();
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        builder.Services.AddHttpClient<DiscordOauthService>();
        builder.Services.AddScoped<ScheduledMessageService>();
        builder.Services.AddHttpClient<SongDataService>();
        builder.Services.AddScoped<MessageCountService>();
        builder.Services.AddScoped<SecretSantaService>();
        builder.Services.AddSingleton<StopWordService>();
        builder.Services.AddTransient<WordCloudService>();
    }
}
