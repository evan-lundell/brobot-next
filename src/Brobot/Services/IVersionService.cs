namespace Brobot.Services;

public interface IVersionService
{
    Task CheckForVersionUpdate(CancellationToken cancellationToken = default);
}