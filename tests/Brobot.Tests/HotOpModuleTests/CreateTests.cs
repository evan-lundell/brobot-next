using Brobot.Models;
using Brobot.Repositories;
using Discord;
using Moq;

namespace Brobot.Tests.HotOpModuleTests;

[TestFixture]
public class CreateTests : HotOpModuleTestBase
{
    private const ulong UserId = 1;
    private const ulong ChannelId = 2;
    private const string Timezone = "America/Chicago";

    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IChannelRepository> _channelRepositoryMock;
    private Mock<IHotOpRepository> _hotOpRepositoryMock;

    [SetUp]
    public void CreateSetup()
    {
        Mock<IUser> userMock = new();
        userMock.SetupGet(u => u.Id).Returns(UserId);
        InteractionContextMock.SetupGet(c => c.User).Returns(userMock.Object);
        Mock<ITextChannel> channelMock = new();
        channelMock.SetupGet(c => c.Id).Returns(ChannelId);
        InteractionContextMock.SetupGet(c => c.Channel).Returns(channelMock.Object);

        _userRepositoryMock = new Mock<IUserRepository>();
        _channelRepositoryMock = new Mock<IChannelRepository>();
        _hotOpRepositoryMock = new Mock<IHotOpRepository>();
        UnitOfWorkMock.SetupGet(uow => uow.Users).Returns(_userRepositoryMock.Object);
        UnitOfWorkMock.SetupGet(uow => uow.Channels).Returns(_channelRepositoryMock.Object);
        UnitOfWorkMock.SetupGet(uow => uow.HotOps).Returns(_hotOpRepositoryMock.Object);
    }

    [Test]
    public async Task StartDateAfterEndDate_RespondsWithMessage()
    {
        await HotOpModule.Create(new DateTime(2026, 10, 2, 20, 0, 0), new DateTime(2026, 10, 1, 20, 0, 0));

        AssertRespondAsyncCalledOnce("Start time must be before end time", true);
        UnitOfWorkMock.VerifyGet(uow => uow.Users, Times.Never);
        AssertHotOpNotSaved();
    }

    [Test]
    public async Task StartDateEqualsEndDate_RespondsWithMessage()
    {
        var time = new DateTime(2026, 10, 1, 20, 0, 0);

        await HotOpModule.Create(time, time);

        AssertRespondAsyncCalledOnce("Start time must be before end time", true);
        UnitOfWorkMock.VerifyGet(uow => uow.Users, Times.Never);
        AssertHotOpNotSaved();
    }

    [Test]
    public async Task UserNotFound_RespondsWithErrorMessage()
    {
        SetupChannel();

        await HotOpModule.Create(new DateTime(2026, 10, 1, 20, 0, 0), new DateTime(2026, 10, 2, 20, 0, 0));

        AssertRespondAsyncCalledOnce("Failed to create hot op", true);
        AssertHotOpNotSaved();
    }

    [Test]
    public async Task ChannelNotFound_RespondsWithErrorMessage()
    {
        SetupUser(Timezone);

        await HotOpModule.Create(new DateTime(2026, 10, 1, 20, 0, 0), new DateTime(2026, 10, 2, 20, 0, 0));

        AssertRespondAsyncCalledOnce("Failed to create hot op", true);
        AssertHotOpNotSaved();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task UserHasNoTimezone_RespondsWithTimezoneMessage(string? timezone)
    {
        SetupUser(timezone);
        SetupChannel();

        await HotOpModule.Create(new DateTime(2026, 10, 1, 20, 0, 0), new DateTime(2026, 10, 2, 20, 0, 0));

        AssertRespondAsyncCalledOnce("You must have a timezone set to create a Hot Op", true);
        AssertHotOpNotSaved();
    }

    [Test]
    public async Task ValidInput_SavesHotOpInUtcAndRespondsWithConfirmation()
    {
        var user = SetupUser(Timezone);
        var channel = SetupChannel();
        HotOpModel? savedHotOp = null;
        _hotOpRepositoryMock.Setup(r => r.Add(It.IsAny<HotOpModel>(), It.IsAny<CancellationToken>()))
            .Callback<HotOpModel, CancellationToken>((hotOp, _) => savedHotOp = hotOp)
            .Returns(Task.CompletedTask);

        // Both dates are in CDT (UTC-5)
        await HotOpModule.Create(new DateTime(2026, 10, 1, 20, 0, 0), new DateTime(2026, 10, 2, 20, 0, 0));

        Assert.That(savedHotOp, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(savedHotOp!.UserId, Is.EqualTo(UserId));
            Assert.That(savedHotOp.DiscordUser, Is.SameAs(user));
            Assert.That(savedHotOp.ChannelId, Is.EqualTo(ChannelId));
            Assert.That(savedHotOp.Channel, Is.SameAs(channel));
            Assert.That(savedHotOp.StartDate, Is.EqualTo(new DateTimeOffset(2026, 10, 2, 1, 0, 0, TimeSpan.Zero)));
            Assert.That(savedHotOp.StartDate.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(savedHotOp.EndDate, Is.EqualTo(new DateTimeOffset(2026, 10, 3, 1, 0, 0, TimeSpan.Zero)));
            Assert.That(savedHotOp.EndDate.Offset, Is.EqualTo(TimeSpan.Zero));
        });
        UnitOfWorkMock.Verify(uow => uow.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        AssertRespondAsyncCalledOnce("Operation Hot TestUser has been created!");
    }

    [Test]
    public async Task SpansDstChange_ConvertsEachDateWithItsOwnOffset()
    {
        SetupUser(Timezone);
        SetupChannel();
        HotOpModel? savedHotOp = null;
        _hotOpRepositoryMock.Setup(r => r.Add(It.IsAny<HotOpModel>(), It.IsAny<CancellationToken>()))
            .Callback<HotOpModel, CancellationToken>((hotOp, _) => savedHotOp = hotOp)
            .Returns(Task.CompletedTask);

        // DST ends 2026-11-01: start is CDT (UTC-5), end is CST (UTC-6)
        await HotOpModule.Create(new DateTime(2026, 10, 31, 20, 0, 0), new DateTime(2026, 11, 2, 20, 0, 0));

        Assert.That(savedHotOp, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(savedHotOp!.StartDate, Is.EqualTo(new DateTimeOffset(2026, 11, 1, 1, 0, 0, TimeSpan.Zero)));
            Assert.That(savedHotOp.EndDate, Is.EqualTo(new DateTimeOffset(2026, 11, 3, 2, 0, 0, TimeSpan.Zero)));
        });
    }

    [Test]
    public async Task ExceptionOccurs_RespondsWithErrorMessage()
    {
        UnitOfWorkMock.SetupGet(uow => uow.Users).Throws(new Exception("Database error"));

        await HotOpModule.Create(new DateTime(2026, 10, 1, 20, 0, 0), new DateTime(2026, 10, 2, 20, 0, 0));

        AssertRespondAsyncCalledOnce("Failed to create hot op", true);
    }

    private DiscordUserModel SetupUser(string? timezone)
    {
        DiscordUserModel user = new()
        {
            Id = UserId,
            Username = "TestUser",
            Timezone = timezone
        };
        _userRepositoryMock.Setup(r => r.GetByIdNoTracking(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        return user;
    }

    private ChannelModel SetupChannel()
    {
        ChannelModel channel = new()
        {
            Id = ChannelId,
            Name = "TestChannel",
            Guild = new GuildModel
            {
                Id = 1,
                Name = "TestGuild"
            }
        };
        _channelRepositoryMock.Setup(r => r.GetById(ChannelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(channel);
        return channel;
    }

    private void AssertHotOpNotSaved()
    {
        _hotOpRepositoryMock.Verify(r => r.Add(It.IsAny<HotOpModel>(), It.IsAny<CancellationToken>()), Times.Never);
        UnitOfWorkMock.Verify(uow => uow.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
