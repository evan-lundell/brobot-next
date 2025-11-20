using Brobot.Configuration;
using Brobot.Models;
using Brobot.Repositories;
using Discord;
using Microsoft.Extensions.Options;

namespace Brobot.Services;

public class VersionService(
    IUnitOfWork uow,
    IDiscordClient client,
    ILogger<VersionService> logger,
    IOptions<GeneralOptions> generalOptions,
    IAssemblyService assemblyService) : IVersionService
{
    public async Task CheckForVersionUpdate()
    {
        logger.LogInformation("Checking for version update");
        var isVersionNew = false;
        var assemblyVersion = assemblyService.GetVersionFromAssembly();

        var currentVersion = await uow.Versions.GetLatestVersion();
        if (currentVersion == null)
        {
            logger.LogInformation("Current version not in database, adding version '{Version}'", assemblyVersion);
            _ = await CreateNewVersion(assemblyVersion);
            isVersionNew = true;
        }
        else if (currentVersion.VersionNumber != assemblyVersion)
        {
            logger.LogInformation("Updated to '{NewVersionNumber}'", assemblyVersion);
            _ = await CreateNewVersion(assemblyVersion);
            isVersionNew = true;
        }
        
        if (isVersionNew && !string.IsNullOrEmpty(generalOptions.Value.ReleaseNotesUrl))
        {
            logger.LogInformation("Sending version upgrade message for version '{Version}'", assemblyVersion);
            await SendVersionUpgradeMessage(assemblyVersion);
        }
        
        logger.LogInformation("Finished checking for version update");
    }
    
    private async Task<VersionModel> CreateNewVersion(string versionName)
    {
        VersionModel newVersion = new()
        {
            VersionNumber = versionName,
            VersionDate = DateTimeOffset.UtcNow
        };
        await uow.Versions.Add(newVersion);
        await uow.CompleteAsync();
        return newVersion;
    }

    private async Task SendVersionUpgradeMessage(string versionName)
    {
        var guilds = (await client.GetGuildsAsync())
            .ToDictionary(x => x.Id);
        var guildIds = guilds.Keys.ToList();
        var guildModels = await uow.Guilds.Find(g => guildIds.Contains(g.Id));
        foreach (var guildModel in guildModels)
        {
            if (guildModel.PrimaryChannelId == null)
            {
                continue;
            }

            var channel = await guilds[guildModel.Id].GetChannelAsync(guildModel.PrimaryChannelId.Value);
            if (channel is ITextChannel textChannel)
            {
                var releaseNotesFullUrl = $"{generalOptions.Value.ReleaseNotesUrl}/v{versionName}";
                await textChannel.SendMessageAsync(
                    $"Brobot {versionName} just dropped! Check out the release notes here: {releaseNotesFullUrl}");
            }
        }
    }
}