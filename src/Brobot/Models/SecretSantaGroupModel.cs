// ReSharper disable VirtualMemberCallInConstructor

namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class SecretSantaGroupModel
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public virtual ICollection<SecretSantaGroupUserModel> SecretSantaGroupUsers { get; set; }
    public virtual ICollection<SecretSantaPairModel> SecretSantaPairs { get; set; }

    public SecretSantaGroupModel()
    {
        SecretSantaGroupUsers = new HashSet<SecretSantaGroupUserModel>();
        SecretSantaPairs = new HashSet<SecretSantaPairModel>();
    }
}