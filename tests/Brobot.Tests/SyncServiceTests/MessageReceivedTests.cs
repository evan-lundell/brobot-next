using Brobot.Configuration;
using Brobot.Services;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

public class MessageReceivedTests : SyncServiceTestsBase
{
    private Mock<IMessageCountService> _messageCountService;
    
    [SetUp]
    public override void Setup()
    {
        Mock<IServiceProvider> servicesMock = new();
        Mock<IServiceScopeFactory> scopeFactoryMock = new();
        Mock<IServiceScope> scopeMock = new();
        Mock<IServiceProvider> scopedProviderMock = new();
        _messageCountService = new Mock<IMessageCountService>();
        
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
            .Setup(s => s.GetService(typeof(IMessageCountService)))
            .Returns(_messageCountService.Object);
        
        Mock<IConfiguration> configurationMock = new();
        configurationMock.Setup(c => c["FixTwitterLinks"]).Returns(bool.TrueString);

        _messageCountService.Setup(mcs =>
            mcs.AddToDailyCount(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<DateOnly?>()));
        
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
        throw new  NotImplementedException();
    }

    [Test]
    public async Task UserIsBot_DoesNothing()
    {
        Mock<IMessage> messageMock = new();
        Mock<IUser> authorMock = new();
        Mock<IMessageChannel> channelMock = new();
        channelMock.Setup(c => c.SendMessageAsync(
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
        
        authorMock.Setup(a => a.IsBot).Returns(true);
        messageMock.Setup(m => m.Author).Returns(authorMock.Object);
        messageMock.SetupGet(m => m.Channel).Returns(channelMock.Object);
        
        await SyncService.MessageReceived(messageMock.Object);

        using (Assert.EnterMultipleScope())
        {
            _messageCountService.Verify(mcs => mcs.AddToDailyCount(
                It.IsAny<ulong>(), 
                It.IsAny<ulong>(), 
                It.IsAny<DateOnly?>()), Times.Never);
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
                It.IsAny<PollProperties>()), Times.Never);
        }
    }

    [Test]
    public async Task MessageReceived_AddedToMessageCount()
    {
        const ulong userId = 1;
        const ulong channelId = 1;
        Mock<IMessage> messageMock = new();
        Mock<IUser> authorMock = new();
        Mock<IMessageChannel> channelMock = new();
        channelMock.Setup(c => c.SendMessageAsync(
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
        channelMock.Setup(c => c.Id).Returns(channelId);
        authorMock.Setup(a => a.IsBot).Returns(false);
        authorMock.Setup(a => a.Id).Returns(userId);
        messageMock.Setup(m => m.Author).Returns(authorMock.Object);
        messageMock.SetupGet(m => m.Channel).Returns(channelMock.Object);
        messageMock.SetupGet(m => m.Content).Returns("");
        
        await SyncService.MessageReceived(messageMock.Object);
        
        _messageCountService.Verify(mcs => mcs.AddToDailyCount(userId, channelId, null), Times.Once);
    }

    [Test]
    [TestCase("good bot", "Thanks! :robot:")]
    [TestCase("bad bot", ":middle_finger:")]
    public async Task MessageReceived_Responds(string message, string expected)
    {
        const ulong userId = 1;
        const ulong channelId = 1;
        Mock<IMessage> messageMock = new();
        Mock<IUser> authorMock = new();
        Mock<IMessageChannel> channelMock = new();
        channelMock.Setup(c => c.SendMessageAsync(
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
        channelMock.Setup(c => c.Id).Returns(channelId);
        authorMock.Setup(a => a.IsBot).Returns(false);
        authorMock.Setup(a => a.Id).Returns(userId);
        messageMock.Setup(m => m.Author).Returns(authorMock.Object);
        messageMock.SetupGet(m => m.Channel).Returns(channelMock.Object);
        messageMock.SetupGet(m => m.Content).Returns(message);
        
        await SyncService.MessageReceived(messageMock.Object);

        using (Assert.EnterMultipleScope())
        {
            channelMock.Verify(c => c.SendMessageAsync(
                expected,
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

    [Test]
    [TestCase("https://twitter.com/test")]
    [TestCase("https://x.com/test")]
    public async Task ContainsTwitterLink_FixesTwitterLink(string message)
    {
        const ulong userId = 1;
        const ulong channelId = 1;
        Mock<IMessage> messageMock = new();
        Mock<IUser> authorMock = new();
        Mock<IMessageChannel> channelMock = new();
        channelMock.Setup(c => c.SendMessageAsync(
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
        channelMock.Setup(c => c.Id).Returns(channelId);
        authorMock.Setup(a => a.IsBot).Returns(false);
        authorMock.Setup(a => a.Id).Returns(userId);
        messageMock.Setup(m => m.Author).Returns(authorMock.Object);
        messageMock.SetupGet(m => m.Channel).Returns(channelMock.Object);
        messageMock.SetupGet(m => m.Content).Returns(message);
        
        await SyncService.MessageReceived(messageMock.Object);

        using (Assert.EnterMultipleScope())
        {
            channelMock.Verify(c => c.SendMessageAsync(
                "https://vxtwitter.com/test",
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