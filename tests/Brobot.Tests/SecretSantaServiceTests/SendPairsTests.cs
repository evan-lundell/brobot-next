using Brobot.Shared.Responses;
using Discord;
using Moq;

namespace Brobot.Tests.SecretSantaServiceTests;

public class SendPairsTests : SecretSantaServiceTestsBase
{
    [Test]
    public async Task PairsGetSent()
    {
        const ulong giverUserId = 1UL;
        const string recipientUsername = "Recipient";
        var pairs = new List<SecretSantaPairResponse>
        {
            new()
            {
                Giver = new UserResponse
                {
                    Id = giverUserId,
                    Username = "Giver"
                },
                Recipient = new UserResponse
                {
                    Id = 2UL,
                    Username = recipientUsername,
                },
                Year = 2025
            }
        };
        Mock<IUser> giverMock = new();
        Mock<IDMChannel> dmChannelMock = new();
        dmChannelMock.Setup(x => x.SendMessageAsync(
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
            It.IsAny<PollProperties>()))
            .Verifiable();
        giverMock.Setup(g => g.CreateDMChannelAsync(It.IsAny<RequestOptions>()))
            .ReturnsAsync(dmChannelMock.Object);
        DiscordClientMock.Setup(c => c.GetUserAsync(
            giverUserId,
            It.IsAny<CacheMode>(),
            It.IsAny<RequestOptions>()
        )).ReturnsAsync(giverMock.Object);

        await SecretSantaService.SendPairs(pairs);

        dmChannelMock.Verify(d => d.SendMessageAsync(
            $"You have {recipientUsername}! :santa:",
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