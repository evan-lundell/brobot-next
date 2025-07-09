using Brobot.Contexts;
using Microsoft.EntityFrameworkCore.Storage;

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
        DailyMessageCounts = new DailyMessageCountRepository(context);
        SecretSantaGroups = new SecretSantaGroupRepository(context);
        StopWords = new StopWordRepository(context);
    }

    public IUserRepository Users { get; }
    public IChannelRepository Channels { get; }
    public IGuildRepository Guilds { get; }
    public IScheduledMessageRepository ScheduledMessages { get; }

    public IHotOpRepository HotOps { get; }

    public IHotOpSessionRepository HotOpSessions { get; }
    public IDailyMessageCountRepository DailyMessageCounts { get; }
    public ISecretSantaGroupRepository SecretSantaGroups { get; }
    public IStopWordRepository StopWords { get; }

    public Task<int> CompleteAsync()
    {
        return _context.SaveChangesAsync();
    }

    public Task<IDbContextTransaction> BeginTransaction() => _context.Database.BeginTransactionAsync();

    public Task CommitTransaction(IDbContextTransaction transaction) => transaction.CommitAsync();

#pragma warning disable CA1816
    public void Dispose()
#pragma warning restore CA1816
    {
        _context.Dispose();
    }
}