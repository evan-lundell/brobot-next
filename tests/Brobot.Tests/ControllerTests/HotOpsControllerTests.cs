using System.Linq.Expressions;
using System.Security.Claims;
using Brobot.Controllers;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.Shared;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Brobot.Tests.ControllerTests;

[TestFixture]
public class HotOpsControllerTests
{
    private const int ExistingHotOpId = 10;
    private const int OtherUsersHotOpId = 20;
    private const ulong MissingUserId = 999;
    private static readonly DateTimeOffset OnTheMinute = new(2026, 12, 1, 20, 0, 0, TimeSpan.Zero);

    private Mock<IUnitOfWork> _uowMock = null!;
    private Mock<IHotOpRepository> _hotOpRepoMock = null!;
    private Mock<IUserRepository> _userRepoMock = null!;
    private HotOpsController _controller = null!;
    private DiscordUserModel _user = null!;
    private DiscordUserModel _otherUser = null!;
    private DiscordUserModel _archivedUser = null!;
    private ChannelModel _channel = null!;
    private HotOpModel _existingHotOp = null!;
    private HotOpModel _otherUsersHotOp = null!;

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
        _otherUser = new DiscordUserModel { Id = 2, Username = "Other", Timezone = null };
        _archivedUser = new DiscordUserModel { Id = 3, Username = "Archived", Timezone = null, Archived = true };
        _existingHotOp = new HotOpModel
        {
            Id = ExistingHotOpId,
            UserId = _user.Id,
            DiscordUser = _user,
            Channel = _channel,
            ChannelId = _channel.Id,
            StartDate = OnTheMinute,
            EndDate = OnTheMinute.AddHours(2)
        };
        _otherUsersHotOp = new HotOpModel
        {
            Id = OtherUsersHotOpId,
            UserId = _otherUser.Id,
            DiscordUser = _otherUser,
            Channel = _channel,
            ChannelId = _channel.Id,
            StartDate = OnTheMinute,
            EndDate = OnTheMinute.AddHours(2)
        };

        _hotOpRepoMock = new Mock<IHotOpRepository>();
        _hotOpRepoMock
            .Setup(r => r.GetById(ExistingHotOpId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingHotOp);
        _hotOpRepoMock
            .Setup(r => r.GetById(OtherUsersHotOpId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_otherUsersHotOp);
        var channelRepoMock = new Mock<IChannelRepository>();
        channelRepoMock
            .Setup(r => r.GetById(_channel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_channel);
        _userRepoMock = new Mock<IUserRepository>();
        _userRepoMock
            .Setup(r => r.GetById(_otherUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_otherUser);
        _userRepoMock
            .Setup(r => r.GetById(_archivedUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_archivedUser);

        _uowMock = new Mock<IUnitOfWork>();
        _uowMock.SetupGet(u => u.HotOps).Returns(_hotOpRepoMock.Object);
        _uowMock.SetupGet(u => u.Channels).Returns(channelRepoMock.Object);
        _uowMock.SetupGet(u => u.Users).Returns(_userRepoMock.Object);
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

    // ---- Owner assignment on create ----

    [Test]
    public async Task CreateHotOp_AdminCanCreateForAnotherUser()
    {
        SignInAsAdmin();
        HotOpModel? added = null;
        _hotOpRepoMock
            .Setup(r => r.Add(It.IsAny<HotOpModel>(), It.IsAny<CancellationToken>()))
            .Callback<HotOpModel, CancellationToken>((model, _) => added = model);

        var result = await _controller.CreateHotOp(ValidRequest(userId: _otherUser.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(added, Is.Not.Null);
            Assert.That(added!.UserId, Is.EqualTo(_otherUser.Id));
            Assert.That(added.DiscordUser, Is.SameAs(_otherUser));
            Assert.That(ResponseOf(result).DiscordUser.Id, Is.EqualTo(_otherUser.Id));
        });
    }

    [Test]
    public async Task CreateHotOp_NonAdminCannotCreateForAnotherUser()
    {
        var result = await _controller.CreateHotOp(ValidRequest(userId: _otherUser.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
            _hotOpRepoMock.Verify(r => r.Add(It.IsAny<HotOpModel>(), It.IsAny<CancellationToken>()), Times.Never);
            _userRepoMock.Verify(r => r.GetById(It.IsAny<ulong>(), It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    [Test]
    public async Task CreateHotOp_NonAdminCanPassTheirOwnUserId()
    {
        HotOpModel? added = null;
        _hotOpRepoMock
            .Setup(r => r.Add(It.IsAny<HotOpModel>(), It.IsAny<CancellationToken>()))
            .Callback<HotOpModel, CancellationToken>((model, _) => added = model);

        var result = await _controller.CreateHotOp(ValidRequest(userId: _user.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(added?.UserId, Is.EqualTo(_user.Id));
            Assert.That(added?.DiscordUser, Is.SameAs(_user));
        });
    }

    [TestCase(3UL, TestName = "CreateHotOp_RejectsInvalidOwner_Archived")]
    [TestCase(MissingUserId, TestName = "CreateHotOp_RejectsInvalidOwner_Missing")]
    public async Task CreateHotOp_RejectsInvalidOwner(ulong userId)
    {
        SignInAsAdmin();

        var result = await _controller.CreateHotOp(ValidRequest(userId: userId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            _hotOpRepoMock.Verify(r => r.Add(It.IsAny<HotOpModel>(), It.IsAny<CancellationToken>()), Times.Never);
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    // ---- Update ----

    [Test]
    public async Task UpdateHotOp_NonAdminCannotEditAnotherUsersHotOp()
    {
        var result = await _controller.UpdateHotOp(OtherUsersHotOpId, ValidRequest());

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    [Test]
    public async Task UpdateHotOp_AdminCanEditAnotherUsersHotOp_KeepsOwnerWhenUserIdOmitted()
    {
        SignInAsAdmin();

        var result = await _controller.UpdateHotOp(OtherUsersHotOpId, ValidRequest());

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(_otherUsersHotOp.UserId, Is.EqualTo(_otherUser.Id));
            Assert.That(_otherUsersHotOp.DiscordUser, Is.SameAs(_otherUser));
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        });
    }

    [Test]
    public async Task UpdateHotOp_AdminCanReassignOwner()
    {
        SignInAsAdmin();

        var result = await _controller.UpdateHotOp(ExistingHotOpId, ValidRequest(userId: _otherUser.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(_existingHotOp.UserId, Is.EqualTo(_otherUser.Id));
            Assert.That(_existingHotOp.DiscordUser, Is.SameAs(_otherUser));
            Assert.That(ResponseOf(result).DiscordUser.Id, Is.EqualTo(_otherUser.Id));
        });
    }

    [Test]
    public async Task UpdateHotOp_NonAdminCannotReassignTheirOwnHotOp()
    {
        var result = await _controller.UpdateHotOp(ExistingHotOpId, ValidRequest(userId: _otherUser.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
            Assert.That(_existingHotOp.UserId, Is.EqualTo(_user.Id));
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    [Test]
    public async Task UpdateHotOp_RejectsArchivedOwner()
    {
        SignInAsAdmin();

        var result = await _controller.UpdateHotOp(ExistingHotOpId, ValidRequest(userId: _archivedUser.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(_existingHotOp.UserId, Is.EqualTo(_user.Id));
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    [Test]
    public async Task UpdateHotOp_ReturnsNotFoundBeforeCheckingOwner()
    {
        var result = await _controller.UpdateHotOp(12345, ValidRequest(userId: _otherUser.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
            _userRepoMock.Verify(r => r.GetById(It.IsAny<ulong>(), It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    // ---- Delete ----

    [Test]
    public async Task DeleteHotOp_NonAdminCannotDeleteAnotherUsersHotOp()
    {
        var result = await _controller.DeleteHotOp(OtherUsersHotOpId);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<ForbidResult>());
            _hotOpRepoMock.Verify(r => r.Remove(It.IsAny<HotOpModel>()), Times.Never);
        });
    }

    [Test]
    public async Task DeleteHotOp_AdminCanDeleteAnotherUsersHotOp()
    {
        SignInAsAdmin();

        var result = await _controller.DeleteHotOp(OtherUsersHotOpId);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkResult>());
            _hotOpRepoMock.Verify(r => r.Remove(_otherUsersHotOp), Times.Once);
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        });
    }

    // ---- Get (allUsers) ----

    [Test]
    public async Task GetHotOps_NonAdminCannotRequestAllUsers()
    {
        var result = await _controller.GetHotOps(HotOpQueryType.Upcoming, allUsers: true);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
            _hotOpRepoMock.Verify(r => r.Find(It.IsAny<Expression<Func<HotOpModel, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    [TestCase(true, true, TestName = "GetHotOps_Upcoming_AdminWithAllUsers_IncludesOtherUsers")]
    [TestCase(true, false, TestName = "GetHotOps_Upcoming_AdminWithoutAllUsers_OnlyOwnHotOps")]
    [TestCase(false, false, TestName = "GetHotOps_Upcoming_NonAdmin_OnlyOwnHotOps")]
    public async Task GetHotOps_Upcoming_FiltersByOwner(bool isAdmin, bool allUsers)
    {
        if (isAdmin)
        {
            SignInAsAdmin();
        }
        var predicate = CaptureFindPredicate();

        var result = await _controller.GetHotOps(HotOpQueryType.Upcoming, allUsers);

        // Relative to now so these stay "upcoming" no matter when the tests run
        var tomorrow = DateTimeOffset.UtcNow.AddDays(1);
        var ownUpcoming = HotOp(_user, tomorrow);
        var otherUpcoming = HotOp(_otherUser, tomorrow);
        var otherPast = HotOp(_otherUser, DateTimeOffset.UtcNow.AddDays(-1));
        var matches = predicate();
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(matches(ownUpcoming), Is.True);
            Assert.That(matches(otherUpcoming), Is.EqualTo(allUsers));
            Assert.That(matches(otherPast), Is.False);
        });
    }

    // ---- Helpers ----

    private void SignInAsAdmin()
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Role, Constants.AdminRoleName)], authenticationType: "Test"));
    }

    // Returns a thunk because the predicate is only captured once the controller calls Find
    private Func<Func<HotOpModel, bool>> CaptureFindPredicate()
    {
        Expression<Func<HotOpModel, bool>>? captured = null;
        _hotOpRepoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<HotOpModel, bool>>>(), It.IsAny<CancellationToken>()))
            .Callback<Expression<Func<HotOpModel, bool>>, CancellationToken>((expression, _) => captured = expression)
            .ReturnsAsync(new List<HotOpModel>());
        return () => captured?.Compile() ?? throw new AssertionException("Find was not called");
    }

    private HotOpModel HotOp(DiscordUserModel owner, DateTimeOffset start) => new()
    {
        UserId = owner.Id,
        DiscordUser = owner,
        Channel = _channel,
        ChannelId = _channel.Id,
        StartDate = start,
        EndDate = start.AddHours(2)
    };

    private static HotOpResponse ResponseOf(ActionResult<HotOpResponse> result)
        => (HotOpResponse)((OkObjectResult)result.Result!).Value!;

    private HotOpRequest ValidRequest(ulong? userId = null) => Request(OnTheMinute, OnTheMinute.AddHours(2)) with { UserId = userId };

    private HotOpRequest Request(DateTimeOffset start, DateTimeOffset end) => new()
    {
        ChannelId = _channel.Id,
        StartDate = start,
        EndDate = end
    };
}
