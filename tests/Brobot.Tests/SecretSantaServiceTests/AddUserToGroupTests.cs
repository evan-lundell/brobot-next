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
        var user = new DiscordUserResponse
        {
            Id = 7,
            Username = "User 7"
        };
        
        var group = await SecretSantaService.AddUserToGroup(1, user);
        var groupModel = await Context.SecretSantaGroups
            .Include(ssg => ssg.SecretSantaGroupUsers)
            .ThenInclude(ssgu => ssgu.DiscordUser)
            .SingleAsync(ssg => ssg.Id == 1);
        
        Assert.That(group.Users.Count, Is.EqualTo(7));
        Assert.That(groupModel.SecretSantaGroupUsers.Count, Is.EqualTo(7));
    }
    
    [Test]
    public async Task ThrowsExceptionWhenGroupDoesNotExist()
    {
        var user = new DiscordUserResponse
        {
            Id = 7,
            Username = "User 7"
        };
        
        await Assert.ThrowsAsync<ModelNotFoundException<SecretSantaGroupModel, int>>(async () =>
        {
            await SecretSantaService.AddUserToGroup(3, user);
        });
    }
    
    [Test]
    public async Task ThrowsExceptionWhenUserAlreadyInGroup()
    {
        var user = new DiscordUserResponse
        {
            Id = 1,
            Username = "User 1"
        };
        
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await SecretSantaService.AddUserToGroup(1, user);
        });
    }
    
    [Test]
    public async Task ThrowsExceptionWhenUserDoesNotExist()
    {
        var user = new DiscordUserResponse
        {
            Id = 15,
            Username = "User 15"
        };
        
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await SecretSantaService.AddUserToGroup(1, user);
        });
    }
}