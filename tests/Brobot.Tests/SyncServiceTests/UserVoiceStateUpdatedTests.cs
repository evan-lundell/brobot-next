using Brobot.Configuration;
using Brobot.Services;
using Brobot.Shared;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

public class UserVoiceStateUpdatedTests : SyncServiceTestsBase
{
    private Mock<IHotOpService> _hotOpServiceMock;
    
    [SetUp]
    public override void Setup()
    {
        Mock<IServiceProvider> servicesMock = new();
        Mock<IServiceScopeFactory> scopeFactoryMock = new();
        Mock<IServiceScope> scopeMock = new();
        Mock<IServiceProvider> scopedProviderMock = new();
        _hotOpServiceMock = new Mock<IHotOpService>();

        // services.GetRequiredService<IServiceScopeFactory>() returns scopeFactory
        servicesMock
            .Setup(s => s.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactoryMock.Object);

        // scopeFactory.CreateScope() returns scope
        scopeFactoryMock
            .Setup(f => f.CreateScope())
            .Returns(scopeMock.Object);

        // scope.ServiceProvider returns scoped provider
        scopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(scopedProviderMock.Object);

        // scoped provider returns hotOpService
        scopedProviderMock
            .Setup(s => s.GetService(typeof(IHotOpService)))
            .Returns(_hotOpServiceMock.Object);
        
        GeneralOptions generalOptions = new()
        {
            FixTwitterLinks = true,
            VersionFilePath = "./version.txt"
        };
        SyncService = new SyncService(
            servicesMock.Object,
            Mock.Of<IDiscordClient>(),
            Mock.Of<ILogger<SyncService>>(),
            Options.Create(generalOptions));
    }

    [TearDown]
    public override void TearDown()
    {
    }
    
    protected override void SetupDatabase()
    {
        throw new NotImplementedException();
    }

    [Test]
    public async Task UserJoinedChannel_CallsHotOpServiceWithConnected()
    {
        const ulong connectedUser1Id = 123UL;
        const ulong connectedUser2Id = 456UL;
        const ulong ownerId = 999UL;
        // Mock VoiceChannel
        Mock<IVoiceChannel> voiceChannelMock = new();
        List<IReadOnlyCollection<IGuildUser>> voiceUsers =
            [[Mock.Of<IGuildUser>(u => u.Id == connectedUser1Id), Mock.Of<IGuildUser>(u => u.Id == connectedUser2Id)]];
        voiceChannelMock
            .Setup(vc => vc.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(voiceUsers.ToAsyncEnumerable());

        Mock<IVoiceState> previousVoiceStateMock = new();
        previousVoiceStateMock.SetupGet(vs => vs.VoiceChannel).Returns((IVoiceChannel)null!);

        Mock<IVoiceState> currentVoiceStateMock = new();
        currentVoiceStateMock.SetupGet(vs => vs.VoiceChannel).Returns(voiceChannelMock.Object);

        Mock<IUser> userMock = new();
        userMock.SetupGet(u => u.Id).Returns(ownerId);

        // Act
        await SyncService.UserVoiceStateUpdated(
            userMock.Object,
            previousVoiceStateMock.Object,
            currentVoiceStateMock.Object
        );

        // Assert
        using (Assert.EnterMultipleScope())
        {
            _hotOpServiceMock.Verify(h => h.UpdateHotOps(
                ownerId,
                UserVoiceStateAction.Connected,
                It.Is<List<ulong>>(l => l.SequenceEqual(new[] { connectedUser1Id, connectedUser2Id }))
            ), Times.Once);
            _hotOpServiceMock.Verify(h => h.UpdateHotOps(
                ownerId,
                UserVoiceStateAction.Disconnected,
                It.Is<List<ulong>>(l => l.SequenceEqual(new[] { connectedUser1Id, connectedUser2Id }))
            ), Times.Never);
        }
    }

    [Test]
    public async Task UserLeftChannel_CallsHotOpServiceWithDisconnected()
    {
        const ulong connectedUser1Id = 123UL;
        const ulong connectedUser2Id = 456UL;
        const ulong ownerId = 999UL;
        // Mock VoiceChannel
        Mock<IVoiceChannel> voiceChannelMock = new();
        List<IReadOnlyCollection<IGuildUser>> voiceUsers =
            [[Mock.Of<IGuildUser>(u => u.Id == connectedUser1Id), Mock.Of<IGuildUser>(u => u.Id == connectedUser2Id)]];
        voiceChannelMock
            .Setup(vc => vc.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(voiceUsers.ToAsyncEnumerable());

        Mock<IVoiceState> previousVoiceStateMock = new();
        previousVoiceStateMock.SetupGet(vs => vs.VoiceChannel).Returns(voiceChannelMock.Object);

        Mock<IVoiceState> currentVoiceStateMock = new();
        currentVoiceStateMock.SetupGet(vs => vs.VoiceChannel).Returns((IVoiceChannel)null!);

        Mock<IUser> userMock = new();
        userMock.SetupGet(u => u.Id).Returns(ownerId);

        // Act
        await SyncService.UserVoiceStateUpdated(
            userMock.Object,
            previousVoiceStateMock.Object,
            currentVoiceStateMock.Object
        );

        // Assert
        using (Assert.EnterMultipleScope())
        {
            _hotOpServiceMock.Verify(h => h.UpdateHotOps(
                ownerId,
                UserVoiceStateAction.Disconnected,
                It.Is<List<ulong>>(l => l.SequenceEqual(new[] { connectedUser1Id, connectedUser2Id }))
            ), Times.Once);
            _hotOpServiceMock.Verify(h => h.UpdateHotOps(
                ownerId,
                UserVoiceStateAction.Connected,
                It.Is<List<ulong>>(l => l.SequenceEqual(new[] { connectedUser1Id, connectedUser2Id }))
            ), Times.Never);
        }
    }
}