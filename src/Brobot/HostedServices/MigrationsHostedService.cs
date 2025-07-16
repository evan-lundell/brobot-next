using Brobot.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Brobot.HostedServices;

public class MigrationsHostedService(
    IServiceProvider serviceProvider,
    IWebHostEnvironment environment,
    ILogger<MigrationsHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (environment.IsProduction())
        {
            logger.LogInformation("Starting migrations");
            using var scope = serviceProvider.CreateScope();
            var usersDb = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
            await usersDb.Database.MigrateAsync(cancellationToken);

            var brobotDb = scope.ServiceProvider.GetRequiredService<BrobotDbContext>();
            await brobotDb.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Migrations finished");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}