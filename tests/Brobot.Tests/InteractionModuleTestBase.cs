using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Moq;

namespace Brobot.Tests;

[TestFixture]
public abstract class InteractionModuleTestBase<TModule> where TModule : IInteractionModuleBase
{
    private TModule? _module;

    protected Mock<IInteractionContext> InteractionContextMock { get; private set; }
    protected Mock<IDiscordInteraction> DiscordInteractionMock { get; private set; }

    protected TModule Module => _module
        ?? throw new InvalidOperationException($"Call {nameof(InitializeModule)} from the derived [SetUp].");

    [SetUp]
    public void BaseSetup()
    {
        Mock<DiscordSocketClient> discordSocketClientMock = new();
        DiscordInteractionMock = new Mock<IDiscordInteraction>();
        InteractionContextMock = new Mock<IInteractionContext>();
        InteractionContextMock.SetupGet(x => x.Client).Returns(discordSocketClientMock.Object);
        InteractionContextMock.SetupGet(x => x.Interaction).Returns(DiscordInteractionMock.Object);
    }

    /// <summary>
    /// Sets the module under test and wires up its interaction context.
    /// Call from the derived class's [SetUp], which NUnit runs after <see cref="BaseSetup"/>.
    /// </summary>
    protected void InitializeModule(TModule module)
    {
        module.SetContext(InteractionContextMock.Object);
        _module = module;
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
