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
    private ChannelModel _usersOnlyChannel = null!;
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
        _usersOnlyChannel = new ChannelModel
        {
            Id = 200,
            Name = "User's Channel",
            Guild = _channel.Guild,
            GuildId = 1
        };
        AddMember(_channel, _user);
        AddMember(_channel, _otherUser);
        AddMember(_usersOnlyChannel, _user);
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
            .Setup(r => r.GetByIdWithChannelUsers(_channel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_channel);
        channelRepoMock
            .Setup(r => r.GetByIdWithChannelUsers(_usersOnlyChannel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_usersOnlyChannel);
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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [TestCaseSource(nameof(InvalidTimes))]
    public async Task UpdateHotOp_RejectsTimesNotOnTheMinute(DateTimeOffset start, DateTimeOffset end)
    {
        var result = await _controller.UpdateHotOp(ExistingHotOpId, Request(start, end));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    public async Task CreateHotOp_AcceptsTimesOnTheMinute()
    {
        var result = await _controller.CreateHotOp(Request(OnTheMinute, OnTheMinute.AddHours(2)));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Test]
    public async Task UpdateHotOp_AcceptsTimesOnTheMinute()
    {
        var result = await _controller.UpdateHotOp(ExistingHotOpId, Request(OnTheMinute.AddDays(1), OnTheMinute.AddDays(1).AddHours(3)));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(added, Is.Not.Null);
            Assert.That(added!.UserId, Is.EqualTo(_otherUser.Id));
            Assert.That(added.DiscordUser, Is.SameAs(_otherUser));
            Assert.That(ResponseOf(result).DiscordUser.Id, Is.EqualTo(_otherUser.Id));
        }
    }

    [Test]
    public async Task CreateHotOp_NonAdminCannotCreateForAnotherUser()
    {
        var result = await _controller.CreateHotOp(ValidRequest(userId: _otherUser.Id));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
            _hotOpRepoMock.Verify(r => r.Add(It.IsAny<HotOpModel>(), It.IsAny<CancellationToken>()), Times.Never);
            _userRepoMock.Verify(r => r.GetById(It.IsAny<ulong>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    public async Task CreateHotOp_NonAdminCanPassTheirOwnUserId()
    {
        HotOpModel? added = null;
        _hotOpRepoMock
            .Setup(r => r.Add(It.IsAny<HotOpModel>(), It.IsAny<CancellationToken>()))
            .Callback<HotOpModel, CancellationToken>((model, _) => added = model);

        var result = await _controller.CreateHotOp(ValidRequest(userId: _user.Id));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(added?.UserId, Is.EqualTo(_user.Id));
            Assert.That(added?.DiscordUser, Is.SameAs(_user));
        }
    }

    [TestCase(3UL, TestName = "CreateHotOp_RejectsInvalidOwner_Archived")]
    [TestCase(MissingUserId, TestName = "CreateHotOp_RejectsInvalidOwner_Missing")]
    public async Task CreateHotOp_RejectsInvalidOwner(ulong userId)
    {
        SignInAsAdmin();

        var result = await _controller.CreateHotOp(ValidRequest(userId: userId));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            _hotOpRepoMock.Verify(r => r.Add(It.IsAny<HotOpModel>(), It.IsAny<CancellationToken>()), Times.Never);
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    // ---- Update ----

    [Test]
    public async Task UpdateHotOp_NonAdminCannotEditAnotherUsersHotOp()
    {
        var result = await _controller.UpdateHotOp(OtherUsersHotOpId, ValidRequest());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    public async Task UpdateHotOp_AdminCanEditAnotherUsersHotOp_KeepsOwnerWhenUserIdOmitted()
    {
        SignInAsAdmin();

        var result = await _controller.UpdateHotOp(OtherUsersHotOpId, ValidRequest());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(_otherUsersHotOp.UserId, Is.EqualTo(_otherUser.Id));
            Assert.That(_otherUsersHotOp.DiscordUser, Is.SameAs(_otherUser));
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [TestCase(true, TestName = "UpdateHotOp_RejectsChangingOwner_Admin")]
    [TestCase(false, TestName = "UpdateHotOp_RejectsChangingOwner_NonAdmin")]
    public async Task UpdateHotOp_RejectsChangingOwner(bool isAdmin)
    {
        // Hot ops can't be reassigned by anyone; delete and recreate instead
        if (isAdmin)
        {
            SignInAsAdmin();
        }

        var result = await _controller.UpdateHotOp(ExistingHotOpId, ValidRequest(userId: _otherUser.Id));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(_existingHotOp.UserId, Is.EqualTo(_user.Id));
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    public async Task UpdateHotOp_AcceptsCurrentOwnerAsUserId()
    {
        // The frontend sends the existing owner's id on edit
        SignInAsAdmin();

        var result = await _controller.UpdateHotOp(OtherUsersHotOpId, ValidRequest(userId: _otherUser.Id));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(_otherUsersHotOp.UserId, Is.EqualTo(_otherUser.Id));
        }
    }

    [Test]
    public async Task UpdateHotOp_AdminCanEditHotOpOwnedByArchivedUser()
    {
        SignInAsAdmin();
        _otherUser.Archived = true;

        var result = await _controller.UpdateHotOp(OtherUsersHotOpId, ValidRequest(userId: _otherUser.Id));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Test]
    public async Task UpdateHotOp_ReturnsNotFoundBeforeCheckingOwner()
    {
        var result = await _controller.UpdateHotOp(12345, ValidRequest(userId: _otherUser.Id));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
            _userRepoMock.Verify(r => r.GetById(It.IsAny<ulong>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    // ---- Delete ----

    [Test]
    public async Task DeleteHotOp_NonAdminCannotDeleteAnotherUsersHotOp()
    {
        var result = await _controller.DeleteHotOp(OtherUsersHotOpId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.InstanceOf<ForbidResult>());
            _hotOpRepoMock.Verify(r => r.Remove(It.IsAny<HotOpModel>()), Times.Never);
        }
    }

    [Test]
    public async Task DeleteHotOp_AdminCanDeleteAnotherUsersHotOp()
    {
        SignInAsAdmin();

        var result = await _controller.DeleteHotOp(OtherUsersHotOpId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.InstanceOf<OkResult>());
            _hotOpRepoMock.Verify(r => r.Remove(_otherUsersHotOp), Times.Once);
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    // ---- Get (allUsers) ----

    [Test]
    public async Task GetHotOps_NonAdminCannotRequestAllUsers()
    {
        var result = await _controller.GetHotOps(HotOpQueryType.Upcoming, allUsers: true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
            _hotOpRepoMock.Verify(r => r.GetUpcomingHotOps(It.IsAny<ulong?>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [TestCase(true, true, null, TestName = "GetHotOps_Upcoming_AdminWithAllUsers_QueriesEveryOwner")]
    [TestCase(true, false, 1UL, TestName = "GetHotOps_Upcoming_AdminWithoutAllUsers_QueriesOwnHotOps")]
    [TestCase(false, false, 1UL, TestName = "GetHotOps_Upcoming_NonAdmin_QueriesOwnHotOps")]
    public async Task GetHotOps_Upcoming_QueriesByOwner(bool isAdmin, bool allUsers, ulong? expectedOwnerId)
    {
        if (isAdmin)
        {
            SignInAsAdmin();
        }
        SetUpUpcoming();

        var result = await _controller.GetHotOps(HotOpQueryType.Upcoming, allUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            _hotOpRepoMock.Verify(r => r.GetUpcomingHotOps(expectedOwnerId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    // ---- Timezones ----
    // America/New_York is UTC-5 in winter and UTC-4 in summer. These dates sit on both sides of DST so that using
    // today's offset instead of the hot op's own date fails at least one case whenever the tests run.

    private static IEnumerable<TestCaseData> WallClockTimes()
    {
        yield return new TestCaseData(new DateTime(2027, 1, 10, 20, 0, 0), new DateTimeOffset(2027, 1, 11, 1, 0, 0, TimeSpan.Zero), TimeSpan.FromHours(-5))
            .SetName("{m}_Winter");
        yield return new TestCaseData(new DateTime(2027, 4, 10, 20, 0, 0), new DateTimeOffset(2027, 4, 11, 0, 0, 0, TimeSpan.Zero), TimeSpan.FromHours(-4))
            .SetName("{m}_Summer");
    }

    [TestCaseSource(nameof(WallClockTimes))]
    public async Task GetHotOps_ReturnsDatesInUsersTimezone(DateTime _, DateTimeOffset storedUtc, TimeSpan expectedOffset)
    {
        _user.Timezone = "America/New_York";
        SetUpUpcoming(HotOp(_user, storedUtc));

        var result = await _controller.GetHotOps(HotOpQueryType.Upcoming);

        var hotOp = ((IEnumerable<HotOpResponse>)((OkObjectResult)result.Result!).Value!).Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(hotOp.StartDate.Offset, Is.EqualTo(expectedOffset));
            Assert.That(hotOp.StartDate.Hour, Is.EqualTo(20), "should be 8 PM local");
            Assert.That(hotOp.StartDate, Is.EqualTo(storedUtc), "should be the same instant");
        }
    }

    [TestCaseSource(nameof(WallClockTimes))]
    public async Task CreateHotOp_ConvertsUsingOffsetOnHotOpsDate(DateTime localStart, DateTimeOffset expectedUtc, TimeSpan expectedOffset)
    {
        _user.Timezone = "America/New_York";
        HotOpModel? added = null;
        _hotOpRepoMock
            .Setup(r => r.Add(It.IsAny<HotOpModel>(), It.IsAny<CancellationToken>()))
            .Callback<HotOpModel, CancellationToken>((model, _) => added = model);

        var result = await _controller.CreateHotOp(WallClockRequest(localStart));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(added?.StartDate, Is.EqualTo(expectedUtc));
            Assert.That(added?.StartDate.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(ResponseOf(result).StartDate.Offset, Is.EqualTo(expectedOffset));
        }
    }

    [TestCaseSource(nameof(WallClockTimes))]
    public async Task UpdateHotOp_ConvertsUsingOffsetOnHotOpsDate(DateTime localStart, DateTimeOffset expectedUtc, TimeSpan expectedOffset)
    {
        _user.Timezone = "America/New_York";

        var result = await _controller.UpdateHotOp(ExistingHotOpId, WallClockRequest(localStart));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_existingHotOp.StartDate, Is.EqualTo(expectedUtc));
            Assert.That(ResponseOf(result).StartDate.Offset, Is.EqualTo(expectedOffset));
            Assert.That(ResponseOf(result).StartDate.Hour, Is.EqualTo(20), "should round-trip to 8 PM local");
        }
    }

    private static readonly DateTimeOffset BrowserLocalTime = new(2027, 1, 10, 20, 0, 0, TimeSpan.FromHours(-5));
    private static readonly DateTimeOffset ExpectedUtc = new(2027, 1, 10, 20, 0, 0, TimeSpan.Zero);

    [Test]
    public async Task CreateHotOp_NoTimezone_StoresWallClockTimeAsUtc()
    {
        HotOpModel? added = null;
        _hotOpRepoMock
            .Setup(r => r.Add(It.IsAny<HotOpModel>(), It.IsAny<CancellationToken>()))
            .Callback<HotOpModel, CancellationToken>((model, _) => added = model);

        await _controller.CreateHotOp(Request(BrowserLocalTime, BrowserLocalTime.AddHours(2)));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(added?.StartDate, Is.EqualTo(ExpectedUtc));
            Assert.That(added?.StartDate.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(added?.EndDate, Is.EqualTo(ExpectedUtc.AddHours(2)));
        }
    }

    [Test]
    public async Task UpdateHotOp_NoTimezone_StoresWallClockTimeAsUtc()
    {
        var result = await _controller.UpdateHotOp(ExistingHotOpId, Request(BrowserLocalTime, BrowserLocalTime.AddHours(2)));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(_existingHotOp.StartDate, Is.EqualTo(ExpectedUtc));
            Assert.That(_existingHotOp.StartDate.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(_existingHotOp.EndDate, Is.EqualTo(ExpectedUtc.AddHours(2)));
        }
    }

    // ---- Channel membership ----

    [Test]
    public async Task CreateHotOp_OwnerNotInChannel_ReturnsBadRequest()
    {
        SignInAsAdmin();

        var result = await _controller.CreateHotOp(ValidRequest(userId: _otherUser.Id) with { ChannelId = _usersOnlyChannel.Id });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            _hotOpRepoMock.Verify(r => r.Add(It.IsAny<HotOpModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    public async Task CreateHotOp_OwnerInChannel_Succeeds()
    {
        SignInAsAdmin();

        var result = await _controller.CreateHotOp(ValidRequest(userId: _otherUser.Id));

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateHotOp_MovingToChannelOwnerIsNotIn_ReturnsBadRequest()
    {
        SignInAsAdmin();

        var result = await _controller.UpdateHotOp(OtherUsersHotOpId, ValidRequest() with { ChannelId = _usersOnlyChannel.Id });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(_otherUsersHotOp.ChannelId, Is.EqualTo(_channel.Id));
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    public async Task UpdateHotOp_KeepingChannelOwnerHasLeft_Succeeds()
    {
        SignInAsAdmin();
        _channel.ChannelUsers.Remove(_channel.ChannelUsers.Single(cu => cu.UserId == _otherUser.Id));

        var result = await _controller.UpdateHotOp(OtherUsersHotOpId, ValidRequest());

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task CreateHotOp_ArchivedChannel_ReturnsBadRequest()
    {
        _channel.Archived = true;

        var result = await _controller.CreateHotOp(ValidRequest());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            _hotOpRepoMock.Verify(r => r.Add(It.IsAny<HotOpModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    public async Task UpdateHotOp_MovingToArchivedChannel_ReturnsBadRequest()
    {
        _usersOnlyChannel.Archived = true;

        var result = await _controller.UpdateHotOp(ExistingHotOpId, ValidRequest() with { ChannelId = _usersOnlyChannel.Id });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(_existingHotOp.ChannelId, Is.EqualTo(_channel.Id));
            _uowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    public async Task UpdateHotOp_KeepingChannelThatWasArchived_Succeeds()
    {
        _channel.Archived = true;

        var result = await _controller.UpdateHotOp(ExistingHotOpId, ValidRequest());

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    // ---- Helpers ----

    private static void AddMember(ChannelModel channel, DiscordUserModel user)
        => channel.ChannelUsers.Add(new ChannelDiscordUserModel
        {
            Channel = channel,
            ChannelId = channel.Id,
            DiscordUser = user,
            UserId = user.Id
        });

    private void SignInAsAdmin()
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Role, Constants.AdminRoleName)], authenticationType: "Test"));
    }

    private void SetUpUpcoming(params HotOpModel[] hotOps)
    {
        _hotOpRepoMock
            .Setup(r => r.GetUpcomingHotOps(It.IsAny<ulong?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotOps);
    }

    // The frontend sends local wall-clock time; only the ticks matter, the offset is ignored
    private HotOpRequest WallClockRequest(DateTime localStart) => Request(
        new DateTimeOffset(localStart, TimeSpan.Zero),
        new DateTimeOffset(localStart.AddHours(2), TimeSpan.Zero));

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
