namespace Brobot.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IGuildRepository Guilds { get; }
    IChannelRepository Channels { get; }
    IScheduledMessageRepository ScheduledMessages { get; }
    Task<int> CompleteAsync();
}