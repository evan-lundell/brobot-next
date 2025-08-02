using Brobot.Dtos;
using Discord;
using Moq;

namespace Brobot.Tests.StatsServiceTests;

public class SendStatsTests : StatsServiceTestBase
{
    [Test]
    public async Task ChannelIsNotTextChannel_DoNothing()
    {
        StatsDto stats = new()
        {
            ChannelId = 1UL,
            StartDate = new DateOnly(2025, 8, 1),
            EndDate = new DateOnly(2025, 8, 31),
            MessageCounts =
            [
                new MessageCountDto
                {
                    MessageCount = 10,
                    UserId = 1UL,
                    Username = "test-user"
                }
            ],
            WordCounts =
            [
                new WordCountDto
                {
                    Count = 15,
                    ChannelId = 1UL,
                    Word = "test"
                }
            ]
        };
        WordCloudServiceMock.Setup(wc => wc.GetWordCloud(stats.WordCounts))
            .Verifiable();
        
        await StatsService.SendStats(1UL, stats);
        
        WordCloudServiceMock.Verify(wc => wc.GetWordCloud(stats.WordCounts), Times.Never);
    }

    [Test]
    public async Task NoWordCounts_SendMessageAsyncCalled()
    {
        var channelId = 1UL;
        Mock<ITextChannel> channelMock = new();
        DiscordClientMock.Setup(dc => dc.GetChannelAsync(
                channelId,
                CacheMode.AllowDownload,
                null))
            .ReturnsAsync(channelMock.Object);
        StatsDto stats = new()
        {
            ChannelId = channelId,
            StartDate = new DateOnly(2025, 8, 1),
            EndDate = new DateOnly(2025, 8, 31),
            MessageCounts =
            [
                new MessageCountDto
                {
                    MessageCount = 10,
                    UserId = 1UL,
                    Username = "test-user"
                }
            ]
        };
        
        await StatsService.SendStats(channelId, stats);

        using (Assert.EnterMultipleScope())
        {
            channelMock.Verify(c => c.SendMessageAsync(
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
                It.IsAny<PollProperties>()), Times.Once);
            channelMock.VerifyNoOtherCalls();
        }
    }

    [Test]
    public async Task HasWordCounts_SendFileAsyncCalled()
    {
        var channelId = 1UL;
        Mock<ITextChannel> channelMock = new();
        DiscordClientMock.Setup(dc => dc.GetChannelAsync(channelId, CacheMode.AllowDownload, null))
            .ReturnsAsync(channelMock.Object);
        WordCloudServiceMock.Setup(wcs => wcs.GetWordCloud(It.IsAny<IEnumerable<WordCountDto>>()))
            .ReturnsAsync([0x00000000, 0x00000001, 0x00000010]);
        StatsDto stats = new()
        {
            ChannelId = channelId,
            StartDate = new DateOnly(2025, 8, 1),
            EndDate = new DateOnly(2025, 8, 31),
            MessageCounts =
            [
                new MessageCountDto
                {
                    MessageCount = 10,
                    UserId = 1UL,
                    Username = "test-user"
                }
            ],
            WordCounts =
            [
                new WordCountDto
                {
                    Count = 15,
                    ChannelId = 1UL,
                    Word = "test"
                }
            ]
        };
        
        await StatsService.SendStats(channelId, stats);

        using (Assert.EnterMultipleScope())
        {
            WordCloudServiceMock.Verify(wcs => wcs.GetWordCloud(It.IsAny<IEnumerable<WordCountDto>>()), Times.Once);
            channelMock.Verify(c => c.SendFileAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<Embed>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<bool>(),
                It.IsAny<AllowedMentions>(),
                It.IsAny<MessageReference>(),
                It.IsAny<MessageComponent>(),
                It.IsAny<ISticker[]>(),
                It.IsAny<Embed[]>(),
                It.IsAny<MessageFlags>(),
                It.IsAny<PollProperties>()), Times.Once);
            channelMock.VerifyNoOtherCalls();
            
        }
    }
}