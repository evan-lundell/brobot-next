using Brobot.Contexts;
using Brobot.Controllers;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Brobot.Tests.ControllerTests;

[TestFixture]
public class UsersControllerTests
{
    private BrobotDbContext _context = null!;
    private UnitOfWork _unitOfWork = null!;
    private UsersController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        // A real unit of work over an in-memory database, so the archived filter in the query is what's under test
        var options = new DbContextOptionsBuilder<BrobotDbContext>()
            .UseInMemoryDatabase($"Brobot_{Guid.NewGuid()}")
            .Options;
        _context = new BrobotDbContext(options);
        _unitOfWork = new UnitOfWork(_context);

        // GetAllUsers doesn't touch the UserManager, it's only needed to construct the controller
        var userManager = new UserManager<ApplicationUserModel>(
            Mock.Of<IUserStore<ApplicationUserModel>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        _controller = new UsersController(_unitOfWork, userManager, NullLogger<UsersController>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _unitOfWork.Dispose();
        _context.Dispose();
    }

    [Test]
    public async Task GetAllUsers_ExcludesArchivedUsers()
    {
        _context.DiscordUsers.AddRange(
            new DiscordUserModel { Id = 1, Username = "Active 1" },
            new DiscordUserModel { Id = 2, Username = "Archived", Archived = true },
            new DiscordUserModel { Id = 3, Username = "Active 2" });
        await _context.SaveChangesAsync();

        var result = await _controller.GetAllUsers();

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var users = ((IEnumerable<DiscordUserResponse>)((OkObjectResult)result.Result!).Value!).ToList();
        Assert.That(users.Select(u => u.Id), Is.EquivalentTo(new[] { 1UL, 3UL }));
    }

    [Test]
    public async Task GetAllUsers_AllUsersArchived_ReturnsEmpty()
    {
        _context.DiscordUsers.Add(new DiscordUserModel { Id = 1, Username = "Archived", Archived = true });
        await _context.SaveChangesAsync();

        var result = await _controller.GetAllUsers();

        var users = (IEnumerable<DiscordUserResponse>)((OkObjectResult)result.Result!).Value!;
        Assert.That(users, Is.Empty);
    }
}
