using Brobot.Modules;
using Brobot.Repositories;
using Brobot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class BrobotModuleTestBase
{
    protected Mock<IInteractionContext> InteractionContextMock { get; private set; }
    protected Mock<IDiscordInteraction> DiscordInteractionMock { get; private set; }
    
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; private set; }
    protected Random Random { get; private set; }
    protected Mock<IGiphyService> GiphyServiceMock { get; private set; }
    protected Mock<IRandomFactService> RandomFactServiceMock { get; private set; }
    protected Mock<IDictionaryService> DictionaryServiceMock { get; private set; }
    protected Mock<IHotOpService> HotOpServiceMock { get; private set; }
    protected Mock<IScheduledMessageService> ScheduledMessageServiceMock { get; private set; }
    protected Mock<IAssemblyService> AssemblyServiceMock { get; private set; }
    
    protected BrobotModule BrobotModule { get; private set; }
    
    [SetUp]
    public void Setup()
    {
        Mock<DiscordSocketClient> discordSocketClientMock = new();
        DiscordInteractionMock = new Mock<IDiscordInteraction>();
        InteractionContextMock = new Mock<IInteractionContext>();
        InteractionContextMock.SetupGet(x => x.Client).Returns(discordSocketClientMock.Object);
        InteractionContextMock.SetupGet(x => x.Interaction).Returns(DiscordInteractionMock.Object);
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        Random = new Random(100);
        GiphyServiceMock = new Mock<IGiphyService>();
        RandomFactServiceMock = new Mock<IRandomFactService>();
        DictionaryServiceMock = new Mock<IDictionaryService>();
        HotOpServiceMock = new Mock<IHotOpService>();
        ScheduledMessageServiceMock = new Mock<IScheduledMessageService>();
        AssemblyServiceMock = new Mock<IAssemblyService>();
        
        BrobotModule = new BrobotModule(
            UnitOfWorkMock.Object,
            Random,
            GiphyServiceMock.Object,
            RandomFactServiceMock.Object,
            DictionaryServiceMock.Object,
            HotOpServiceMock.Object,
            ScheduledMessageServiceMock.Object,
            AssemblyServiceMock.Object,
            Mock.Of<ILogger<BrobotModule>>()
        );
        ((IInteractionModuleBase)BrobotModule).SetContext(InteractionContextMock.Object);
    }

    protected void AssertRespondAsyncCalledOnce(string message, bool ephemeral = false)
    {
        DiscordInteractionMock.Verify(x => x.RespondAsync(
            message,
            null,
            false,
            ephemeral,
            null,
            null,
            null,
            null,
            null,
            MessageFlags.None
        ), Times.Once);
    }
}
