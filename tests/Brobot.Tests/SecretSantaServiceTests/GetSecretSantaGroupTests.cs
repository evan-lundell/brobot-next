namespace Brobot.Tests.SecretSantaServiceTests;

[TestFixture]
public class GetSecretSantaGroupTests : SecretSantaServiceTestsBase
{
    [Test]
    public async Task ReturnsSecretSantaGroup()
    {
        var group = await SecretSantaService.GetSecretSantaGroup(1);
        
        Assert.That(group, Is.Not.Null);
        Assert.That(group!.Name, Is.EqualTo("Test Group 1"));
        Assert.That(group.Users.Count, Is.EqualTo(6));
    }
    
    [Test]
    public async Task ReturnsNullWhenGroupDoesNotExist()
    {
        var group = await SecretSantaService.GetSecretSantaGroup(3);
        
        Assert.That(group, Is.Null);
    }
}