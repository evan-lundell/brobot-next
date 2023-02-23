using Brobot.Contexts;

namespace Brobot.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly BrobotDbContext _context;

    public UnitOfWork(BrobotDbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
        Channels = new ChannelRepository(context);
        Guilds = new GuildRepository(context, Channels);
        ScheduledMessages = new ScheduledMessageRepository(context);
        HotOps = new HotOpRepository(context);
        HotOpSessions = new HotOpSessionRepository(context);
    }

    public IUserRepository Users { get; }
    public IChannelRepository Channels { get; }
    public IGuildRepository Guilds { get; }
    public IScheduledMessageRepository ScheduledMessages { get; }

    public IHotOpRepository HotOps { get; }

    public IHotOpSessionRepository HotOpSessions { get; }

    public Task<int> CompleteAsync()
    {
        return _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}