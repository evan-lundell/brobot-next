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

        var client = app.Services.GetRequiredService<BrobotClient>();
        if (string.IsNullOrWhiteSpace(app.Configuration["BrobotToken"]))
        {
            Console.WriteLine("No token provided");
            return;
        }
        await client.StartClientAsync(app.Configuration["BrobotToken"] ?? "");
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
        builder.Services.AddSingleton<BrobotClient>();
    }
}




