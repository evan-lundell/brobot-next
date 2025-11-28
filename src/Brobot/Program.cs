using Brobot.Configuration;
using Brobot.HostedServices;
using Microsoft.OpenApi;
using Serilog;

namespace Brobot;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        CreateServices(builder, args);
        var app = builder.Build();
        ConfigureMiddleware(app);
        app.Run();
    }

    private static void CreateServices(WebApplicationBuilder builder, string[] args)
    {
        builder.Services.AddBrobotOptions(builder.Configuration);
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        var generalOptions = builder.Configuration.GetSection(GeneralOptions.SectionName).Get<GeneralOptions>()!;
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Seq(generalOptions.SeqUrl)
            .CreateLogger();
        
        builder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            })
            .AddBrobotInfrastructure(builder.Configuration)
            .AddUserManagement(builder.Configuration)
            .AddBrobotServices(builder.Configuration)
            .AddHostedService<MigrationsHostedService>()
            .AddSerilog();

        if (!args.Contains("--no-jobs"))
        {
            builder.Services.AddJobs(builder.Configuration);
        }

        if (!args.Contains("--no-bot"))
        {
            builder.Services.AddDiscord(builder.Configuration);
        }
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
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
    }
}
