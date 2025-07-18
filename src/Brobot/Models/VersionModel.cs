namespace Brobot.Models;

public class VersionModel
{
    public int Id { get; set; }

    public required string VersionNumber { get; set; }

    public DateTimeOffset VersionDate { get; set; }
}