using Brobot.Exceptions;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Tests.SecretSantaServiceTests;

[TestFixture]
public class RemoveUserFromGroupTests : SecretSantaServiceTestsBase
{
    [Test]
    public async Task RemovesUserFromGroup()
    {
        var group = await SecretSantaService.RemoveUserFromGroup(1, 1);
        var groupModel = await Context.SecretSantaGroups
            .Include(ssg => ssg.SecretSantaGroupUsers)
            .ThenInclude(ssgu => ssgu.User)
            .SingleAsync(ssg => ssg.Id == 1);
        
        Assert.That(group.Users.Count, Is.EqualTo(5));
        Assert.That(groupModel.SecretSantaGroupUsers.Count, Is.EqualTo(5));
    }
    
    [Test]
    public void ThrowsExceptionWhenGroupDoesNotExist()
    {
        Assert.ThrowsAsync<ModelNotFoundException<SecretSantaGroupModel, int>>(async () =>
        {
            await SecretSantaService.RemoveUserFromGroup(3, 1);
        });
    }
    
    [Test]
    public async Task ReturnsGroupWhenUserNotInGroup()
    {
        var group = await SecretSantaService.RemoveUserFromGroup(1, 7);
        var groupModel = await Context.SecretSantaGroups
            .Include(ssg => ssg.SecretSantaGroupUsers)
            .ThenInclude(ssgu => ssgu.User)
            .SingleAsync(ssg => ssg.Id == 1);
        
        Assert.That(group.Users.Count, Is.EqualTo(6));
        Assert.That(groupModel.SecretSantaGroupUsers.Count, Is.EqualTo(6));
    }
}