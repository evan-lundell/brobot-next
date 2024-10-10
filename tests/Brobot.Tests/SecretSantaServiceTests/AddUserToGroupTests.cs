using Brobot.Exceptions;
using Brobot.Models;
using Brobot.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Tests.SecretSantaServiceTests;

[TestFixture]
public class AddUserToGroupTests : SecretSantaServiceTestsBase
{
    [Test]
    public async Task AddsUserToGroup()
    {
        var user = new UserResponse
        {
            Id = 7,
            Username = "User 7"
        };
        
        var group = await SecretSantaService.AddUserToGroup(1, user);
        var groupModel = await Context.SecretSantaGroups
            .Include(ssg => ssg.SecretSantaGroupUsers)
            .ThenInclude(ssgu => ssgu.User)
            .SingleAsync(ssg => ssg.Id == 1);
        
        Assert.That(group.Users.Count, Is.EqualTo(7));
        Assert.That(groupModel.SecretSantaGroupUsers.Count, Is.EqualTo(7));
    }
    
    [Test]
    public void ThrowsExceptionWhenGroupDoesNotExist()
    {
        var user = new UserResponse
        {
            Id = 7,
            Username = "User 7"
        };
        
        Assert.ThrowsAsync<ModelNotFoundException<SecretSantaGroupModel, int>>(async () =>
        {
            await SecretSantaService.AddUserToGroup(3, user);
        });
    }
    
    [Test]
    public void ThrowsExceptionWhenUserAlreadyInGroup()
    {
        var user = new UserResponse
        {
            Id = 1,
            Username = "User 1"
        };
        
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await SecretSantaService.AddUserToGroup(1, user);
        });
    }
    
    [Test]
    public void ThrowsExceptionWhenUserDoesNotExist()
    {
        var user = new UserResponse
        {
            Id = 15,
            Username = "User 15"
        };
        
        Assert.ThrowsAsync<ModelNotFoundException<UserModel, ulong>>(async () =>
        {
            await SecretSantaService.AddUserToGroup(1, user);
        });
    }
}