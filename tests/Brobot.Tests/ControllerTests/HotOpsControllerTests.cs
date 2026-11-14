using Brobot.Controllers;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.Shared.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Brobot.Tests.ControllerTests;

[TestFixture]
public class HotOpsControllerTests
{
    private const int ExistingHotOpId = 10;
    private static readonly DateTimeOffset OnTheMinute = new(2026, 12, 1, 20, 0, 0, TimeSpan.Zero);

    private Mock<IUnitOfWork> _uowMock = null!;
    private HotOpsController _controller = null!;
    private DiscordUserModel _user = null!;
    private ChannelModel _channel = null!;

    [SetUp]
    public void SetUp()
    {
        // No timezone, so the controller skips timezone conversion and these tests only exercise validation
        _user = new DiscordUserModel { Id = 1, Username = "Owner", Timezone = null };
        _channel = new ChannelModel
        {
            Id = 100,
            Name = "Channel",
            Guild = new GuildModel { Id = 1, Name = "Guild" },
            GuildId = 1
        };
        var existingHotOp = new HotOpModel
        {
            Id = ExistingHotOpId,
            UserId = _user.Id,
            DiscordUser = _user,
            Channel = _channel,
            ChannelId = _channel.Id,
            StartDate = OnTheMinute,
            EndDate = OnTheMinute.AddHours(2)
        };

        var hotOpRepoMock = new Mock<IHotOpRepository>();
        hotOpRepoMock
            .Setup(r => r.GetById(ExistingHotOpId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHotOp);
        var channelRepoMock = new Mock<IChannelRepository>();
        channelRepoMock
            .Setup(r => r.GetById(_channel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_channel);

        _uowMock = new Mock<IUnitOfWork>();
        _uowMock.SetupGet(u => u.HotOps).Returns(hotOpRepoMock.Object);
        _uowMock.SetupGet(u => u.Channels).Returns(channelRepoMock.Object);
        _uowMock.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set(_user);
        _controller = new HotOpsController(
            _uowMock.Object,
            Mock.Of<IHotOpService>(),
            NullLogger<HotOpsController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    private static IEnumerable<TestCaseData> InvalidTimes()
    {
        yield return new TestCaseData(OnTheMinute.AddSeconds(30), OnTheMinute.AddHours(2))
            .SetName("{m}_StartHasSeconds");
        yield return new TestCaseData(OnTheMinute, OnTheMinute.AddHours(2).AddSeconds(30))
            .SetName("{m}_EndHasSeconds");
        yield return new TestCaseData(OnTheMinute.AddMilliseconds(500), OnTheMinute.AddHours(2))
            .SetName("{m}_StartHasOnlyMilliseconds");
        yield return new TestCaseData(OnTheMinute, OnTheMinute.AddHours(2).AddMilliseconds(500))
            .SetName("{m}_EndHasOnlyMilliseconds");
    }

    [TestCaseSource(nameof(InvalidTimes))]
    public async Task CreateHotOp_RejectsTimesNotOnTheMinute(DateTimeOffset start, DateTimeOffset end)
    {
        var result = await _controller.CreateHotOp(Request(start, end));

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    [TestCaseSource(nameof(InvalidTimes))]
    public async Task UpdateHotOp_RejectsTimesNotOnTheMinute(DateTimeOffset start, DateTimeOffset end)
    {
        var result = await _controller.UpdateHotOp(ExistingHotOpId, Request(start, end));

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    [Test]
    public async Task CreateHotOp_AcceptsTimesOnTheMinute()
    {
        var result = await _controller.CreateHotOp(Request(OnTheMinute, OnTheMinute.AddHours(2)));

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        });
    }

    [Test]
    public async Task UpdateHotOp_AcceptsTimesOnTheMinute()
    {
        var result = await _controller.UpdateHotOp(ExistingHotOpId, Request(OnTheMinute.AddDays(1), OnTheMinute.AddDays(1).AddHours(3)));

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        });
    }

    private HotOpRequest Request(DateTimeOffset start, DateTimeOffset end) => new()
    {
        ChannelId = _channel.Id,
        StartDate = start,
        EndDate = end
    };
}
