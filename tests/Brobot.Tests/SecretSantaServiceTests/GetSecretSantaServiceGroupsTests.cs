namespace Brobot.Tests.SecretSantaServiceTests;

[TestFixture]
public class GetSecretSantaServiceGroupsTests : SecretSantaServiceTestsBase
{
    [Test]
    public async Task ReturnsAllSecretSantaGroups()
    {
        var groups = (await SecretSantaService.GetSecretSantaGroups()).ToArray();
        var group1 = groups.First(g => g.Id == 1);
        
        Assert.That(groups.Length, Is.EqualTo(2));
        Assert.That(group1.Name, Is.EqualTo("Test Group 1"));
        Assert.That(group1.Users.Count, Is.EqualTo(6));
    }
}