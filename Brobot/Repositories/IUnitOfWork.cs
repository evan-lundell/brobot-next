namespace Brobot.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IGuildRepository Guilds { get; }
    IChannelRepository Channels { get; }
    IScheduledMessageRepository ScheduledMessages { get; }
    IHotOpRepository HotOps { get; }
    IHotOpSessionRepository HotOpSessions { get; }
    Task<int> CompleteAsync();
}