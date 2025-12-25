// ReSharper disable VirtualMemberCallInConstructor

namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class SecretSantaGroupModel
{
    public int Id { get; init; }
    public required string Name { get; init; }

    public virtual ICollection<SecretSantaGroupDiscordUserModel> SecretSantaGroupUsers { get; init; }
    public virtual ICollection<SecretSantaPairModel> SecretSantaPairs { get; init; }

    public SecretSantaGroupModel()
    {
        SecretSantaGroupUsers = new HashSet<SecretSantaGroupDiscordUserModel>();
        SecretSantaPairs = new HashSet<SecretSantaPairModel>();
    }
}