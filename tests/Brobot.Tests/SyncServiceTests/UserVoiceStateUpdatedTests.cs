using Brobot.Configuration;
using Brobot.Services;
using Brobot.Shared;
using Discord;
using Discord.WebSocket;
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
        Mock<IServiceScopeFactory> scopeFactoryMock = new();
        Mock<IServiceScope> scopeMock = new();
        Mock<IServiceProvider> scopedProviderMock = new();
        _hotOpServiceMock = new Mock<IHotOpService>();

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
            VersionFilePath = "./version.txt",
            SeqUrl = "http://localhost:5341"
        };
        SyncService = new SyncService(
            scopeFactoryMock.Object,
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
        const ulong connectedUser3Id = 789UL;
        const ulong ownerId = 999UL;
        // Mock VoiceChannel
        Mock<IVoiceChannel> voiceChannelMock = new();
        Mock<IGuildUser> guildUser1Mock = new();
        guildUser1Mock.SetupGet(gu => gu.Id).Returns(connectedUser1Id);
        guildUser1Mock.SetupGet(gu => gu.VoiceChannel).Returns(voiceChannelMock.Object);
        Mock<IGuildUser> guildUser2Mock = new();
        guildUser2Mock.SetupGet(gu => gu.Id).Returns(connectedUser2Id);
        guildUser2Mock.SetupGet(gu => gu.VoiceChannel).Returns(voiceChannelMock.Object);
        Mock<IGuildUser> guildUser3Mock = new();
        guildUser3Mock.SetupGet(gu => gu.Id).Returns(connectedUser3Id);
        List<IReadOnlyCollection<IGuildUser>> voiceUsers =
            [[guildUser1Mock.Object, guildUser2Mock.Object, guildUser3Mock.Object]];
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
        const ulong connectedUser3Id = 789UL;
        const ulong ownerId = 999UL;
        // Mock VoiceChannel
        Mock<IVoiceChannel> voiceChannelMock = new();
        Mock<IGuildUser> guildUser1Mock = new();
        guildUser1Mock.SetupGet(gu => gu.Id).Returns(connectedUser1Id);
        guildUser1Mock.SetupGet(gu => gu.VoiceChannel).Returns(voiceChannelMock.Object);
        Mock<IGuildUser> guildUser2Mock = new();
        guildUser2Mock.SetupGet(gu => gu.Id).Returns(connectedUser2Id);
        guildUser2Mock.SetupGet(gu => gu.VoiceChannel).Returns(voiceChannelMock.Object);
        Mock<IGuildUser> guildUser3Mock = new();
        guildUser3Mock.SetupGet(gu => gu.Id).Returns(connectedUser3Id);
        List<IReadOnlyCollection<IGuildUser>> voiceUsers =
            [[guildUser1Mock.Object, guildUser2Mock.Object, guildUser3Mock.Object]];
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
    
    [Test]
    public async Task ThrowsException_LogsError()
    {
        Mock<IServiceScopeFactory> scopeFactoryMock = new();
        Mock<ILogger<SyncService>> loggerMock = new();
        scopeFactoryMock.Setup(sf => sf.CreateScope()).Throws<Exception>();
        SyncService syncService = new(
            scopeFactoryMock.Object,
            Mock.Of<IDiscordClient>(),
            loggerMock.Object,
            Options.Create(new GeneralOptions
            {
                SeqUrl = "http://localhost:5341",
                VersionFilePath = "./version.txt"
            })
        );
        
        await syncService.UserVoiceStateUpdated(
            Mock.Of<IUser>(),
            Mock.Of<IVoiceState>(),
            Mock.Of<IVoiceState>());
        
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing user voice state updated for ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Test]
    public async Task UserConnectsToNewVoiceChannel_BothConnectedAndDisconnectedCalled()
    {
        // Arrange
        const ulong userInPreviousChannelId = 123UL;
        const ulong userInCurrentChannelId = 456UL;
        const ulong connectingUserId = 999UL;
        Mock<IGuildUser> previousVoiceChannelUserMock = new();
        previousVoiceChannelUserMock.SetupGet(u => u.Id).Returns(userInPreviousChannelId);
        Mock<IGuildUser> currentVoiceChannelUserMock = new();
        currentVoiceChannelUserMock.SetupGet(u => u.Id).Returns(userInCurrentChannelId);
        Mock<IGuildUser> connectinngUserMock = new();
        connectinngUserMock.SetupGet(u => u.Id).Returns(connectingUserId);
        Mock<IVoiceChannel> previousVoiceChannelMock = new();
        List<IReadOnlyCollection<IGuildUser>> previousVoiceUsers =
            [[previousVoiceChannelUserMock.Object]];
        previousVoiceChannelMock.Setup(prev => prev.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(previousVoiceUsers.ToAsyncEnumerable());
        Mock<IVoiceState> previousVoiceStateMock = new();
        previousVoiceStateMock.SetupGet(vs => vs.VoiceChannel).Returns(previousVoiceChannelMock.Object);
        previousVoiceChannelUserMock.SetupGet(prev => prev.VoiceChannel).Returns(previousVoiceChannelMock.Object);
        Mock<IVoiceState> currentVoiceStateMock = new();
        Mock<IVoiceChannel> currentVoiceChannelMock = new();
        List<IReadOnlyCollection<IGuildUser>> currentVoiceUsers =
            [[currentVoiceChannelUserMock.Object]];
        currentVoiceChannelMock.Setup(curr => curr.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(currentVoiceUsers.ToAsyncEnumerable());
        currentVoiceStateMock.SetupGet(vs => vs.VoiceChannel).Returns(currentVoiceChannelMock.Object);
        currentVoiceChannelUserMock.SetupGet(curr => curr.VoiceChannel).Returns(currentVoiceChannelMock.Object);
        
        // Act
        await SyncService.UserVoiceStateUpdated(
            connectinngUserMock.Object,
            previousVoiceStateMock.Object,
            currentVoiceStateMock.Object
        );
        
        // Assert
        using (Assert.EnterMultipleScope())
        {
            _hotOpServiceMock.Verify(h => h.UpdateHotOps(
                connectingUserId,
                UserVoiceStateAction.Disconnected,
                It.Is<List<ulong>>(l => l.SequenceEqual(new[] { userInPreviousChannelId }))
            ), Times.Once);
            _hotOpServiceMock.Verify(h => h.UpdateHotOps(
                connectingUserId,
                UserVoiceStateAction.Connected,
                It.Is<List<ulong>>(l => l.SequenceEqual(new[] { userInCurrentChannelId }))
            ), Times.Once);
        }
    }
}