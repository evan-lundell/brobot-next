namespace Brobot.Models;

public class VersionModel
{
    public int Id { get; init; }

    public required string VersionNumber { get; init; }

    public DateTimeOffset VersionDate { get; init; }
}