using System.Security.Claims;
using Brobot.Controllers;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Brobot.Tests.ControllerTests;

[TestFixture]
public class ChannelsControllerTests
{
    private const ulong CallerId = 1;
    private const ulong OtherUserId = 2;

    private Mock<IChannelRepository> _channelRepoMock = null!;
    private ChannelsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _channelRepoMock = new Mock<IChannelRepository>();
        _channelRepoMock
            .Setup(r => r.FindByUser(It.IsAny<ulong>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.SetupGet(u => u.Channels).Returns(_channelRepoMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set(new DiscordUserModel { Id = CallerId, Username = "Caller" });
        _controller = new ChannelsController(uowMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    [TestCase(false, TestName = "GetChannels_NoUserId_ReturnsCallersChannels_NonAdmin")]
    [TestCase(true, TestName = "GetChannels_NoUserId_ReturnsCallersChannels_Admin")]
    public async Task GetChannels_NoUserId_ReturnsCallersChannels(bool isAdmin)
    {
        if (isAdmin)
        {
            SignInAsAdmin();
        }

        var result = await _controller.GetChannels();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            _channelRepoMock.Verify(r => r.FindByUser(CallerId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Test]
    public async Task GetChannels_NonAdminPassingOwnId_ReturnsCallersChannels()
    {
        var result = await _controller.GetChannels(CallerId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            _channelRepoMock.Verify(r => r.FindByUser(CallerId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Test]
    public async Task GetChannels_NonAdminPassingAnotherUser_Forbidden()
    {
        var result = await _controller.GetChannels(OtherUserId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
            _channelRepoMock.Verify(r => r.FindByUser(It.IsAny<ulong>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    public async Task GetChannels_AdminPassingAnotherUser_ReturnsThatUsersChannels()
    {
        SignInAsAdmin();

        var result = await _controller.GetChannels(OtherUserId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            _channelRepoMock.Verify(r => r.FindByUser(OtherUserId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    private void SignInAsAdmin()
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Role, Constants.AdminRoleName)], authenticationType: "Test"));
    }
}
