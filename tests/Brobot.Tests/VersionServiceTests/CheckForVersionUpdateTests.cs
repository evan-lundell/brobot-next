using Brobot.Configuration;
using Brobot.Models;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Brobot.Tests.VersionServiceTests;

public class CheckForVersionUpdateTests : VersionServiceTestsBase
{
    [Test]
    public async Task WhenVersionInDatabaseIsNull_CreatesNewVersion()
    {
        const string versionNumber = "v1.0.0";
        MockClient.Setup(x => x.GetGuildsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ReturnsAsync([]);
        MockAssemblyService.Setup(a => a.GetVersionFromAssembly()).Returns(versionNumber);

        // Act
        await VersionService.CheckForVersionUpdate();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            MockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Current version not in database")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
            var versionModel = await Context.Versions.FirstOrDefaultAsync();
            Assert.That(versionModel, Is.Not.Null);
            Assert.That(versionModel!.VersionNumber, Is.EqualTo(versionNumber));
            MockClient.Verify();
        }

    }

    [Test]
    public async Task WhenVersionIsDifferent_CreatesNewVersion()
    {
        // Arrange
        const string existingVersionNumber = "v1.0.0";
        const string newVersionNumber = "v1.0.1";
        var existingVersionModel = new VersionModel
        {
            VersionNumber = existingVersionNumber,
            VersionDate = DateTimeOffset.Now.AddDays(-1)
        };
        await Context.AddAsync(existingVersionModel);
        await Context.SaveChangesAsync();
        
        MockClient.Setup(x => x.GetGuildsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ReturnsAsync([]);
        MockAssemblyService.Setup(a => a.GetVersionFromAssembly()).Returns(newVersionNumber);
    
        // Act
        await VersionService.CheckForVersionUpdate();
    
        // Assert
        using (Assert.EnterMultipleScope())
        {
            MockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Updated to")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            var newVersionModel = await Context.Versions
                .OrderByDescending(v => v.VersionDate)
                .FirstOrDefaultAsync();
            Assert.That(newVersionModel, Is.Not.Null);
            Assert.That(newVersionModel!.VersionNumber, Is.EqualTo(newVersionNumber));
        }
    }
    
    [Test]
    public async Task WhenVersionIsSame_DoesNotCreateNewVersion()
    {
        // Arrange
        const string versionNumber = "v1.0.0";
        var currentVersion = new VersionModel { VersionNumber = versionNumber, VersionDate = DateTimeOffset.UtcNow.AddDays(-1) };
        await Context.AddAsync(currentVersion);
        await Context.SaveChangesAsync();
        MockAssemblyService.Setup(x => x.GetVersionFromAssembly()).Returns(versionNumber);
    
        // Act
        await VersionService.CheckForVersionUpdate();
    
        // Assert
        var versionCount = await Context.Versions.CountAsync();
        Assert.That(versionCount, Is.EqualTo(1));
    }
    
    [Test]
    public async Task WhenReleaseNotesUrlIsEmpty_DoesNotSendMessage()
    {
        // Arrange
        GeneralOptions newGeneralOptions = new()
        {
            ReleaseNotesUrl = "",
            VersionFilePath = ""
        };
        MockGeneralOptions.SetupGet(go => go.Value).Returns(newGeneralOptions);
        MockAssemblyService.Setup(a => a.GetVersionFromAssembly()).Returns("v1.0.0");
    
        // Act
        await VersionService.CheckForVersionUpdate();
    
        // Assert
        MockClient.Verify(x => x.GetGuildsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()), Times.Never);
    }

    [Test]
    public async Task WhenGuildHasNoPrimaryChannel_DoesNotSendMessage()
    {
        // Arrange
        const string existingVersionNumber = "v1.0.0";
        const string newVersionNumber = "v1.0.1";
        const ulong guildId = 1;
        VersionModel existingVersionModel = new VersionModel
            { VersionNumber = existingVersionNumber, VersionDate = DateTimeOffset.UtcNow.AddDays(-1) };
        GuildModel guild = new()
        {
            Id = guildId,
            Name = "test-guild"
        };
        await Context.AddAsync(existingVersionModel);
        await Context.AddAsync(guild);
        await Context.SaveChangesAsync();
        Mock<IGuild> guildMock = new();
        guildMock.SetupGet(g => g.Id).Returns(guildId);
        MockAssemblyService.Setup(a => a.GetVersionFromAssembly()).Returns(newVersionNumber);
        MockClient.Setup(c => c.GetGuildsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ReturnsAsync([guildMock.Object]);

        // Act
        await VersionService.CheckForVersionUpdate();

        // Assert
        guildMock.Verify(g => g.GetChannelAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()),
            Times.Never);
    }

    [Test]
    public async Task WhenGuildHasPrimaryChannel_SendsMessage()
    {
        // Arrange
        const string existingVersionNumber = "v1.0.0";
        const string newVersionNumber = "v1.0.1";
        const ulong guildId = 1;
        const ulong channelId = 1;
        VersionModel existingVersionModel = new VersionModel
            { VersionNumber = existingVersionNumber, VersionDate = DateTimeOffset.UtcNow.AddDays(-1) };
        GuildModel guild = new()
        {
            Id = guildId,
            Name = "test-guild",
            PrimaryChannelId = channelId
        };
        await Context.AddAsync(existingVersionModel);
        await Context.AddAsync(guild);
        await Context.SaveChangesAsync();
        Mock<ITextChannel> guildChannelMock = new();
        guildChannelMock.SetupGet(g => g.Id).Returns(channelId);
        guildChannelMock.Setup(c => c.SendMessageAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<Embed>(),
            It.IsAny<RequestOptions>(),
            It.IsAny<AllowedMentions>(),
            It.IsAny<MessageReference>(),
            It.IsAny<MessageComponent>(),
            It.IsAny<ISticker[]>(),
            It.IsAny<Embed[]>(),
            It.IsAny<MessageFlags>(),
            It.IsAny<PollProperties>()));
        Mock<IGuild> guildMock = new();
        guildMock.SetupGet(g => g.Id).Returns(guildId);
        guildMock.Setup(g => g.GetChannelAsync(channelId, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ReturnsAsync(guildChannelMock.Object);
        MockAssemblyService.Setup(a => a.GetVersionFromAssembly()).Returns(newVersionNumber);
        MockClient.Setup(c => c.GetGuildsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ReturnsAsync([guildMock.Object]);

        // Act
        await VersionService.CheckForVersionUpdate();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            guildMock.Verify(
                g => g.GetChannelAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()),
                Times.Once);
            var releaseNotesFullUrl = $"https://example.com/releases/v{newVersionNumber}";
            guildChannelMock.Verify(g => g.SendMessageAsync(
                $"Brobot {newVersionNumber} just dropped! Check out the release notes here: {releaseNotesFullUrl}",
                It.IsAny<bool>(),
                It.IsAny<Embed>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<AllowedMentions>(),
                It.IsAny<MessageReference>(),
                It.IsAny<MessageComponent>(),
                It.IsAny<ISticker[]>(),
                It.IsAny<Embed[]>(),
                It.IsAny<MessageFlags>(),
                It.IsAny<PollProperties>()), Times.Once);
        }
    }
}