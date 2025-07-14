using Brobot.Contexts;
using Microsoft.EntityFrameworkCore.Storage;

namespace Brobot.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IGuildRepository Guilds { get; }
    IChannelRepository Channels { get; }
    IScheduledMessageRepository ScheduledMessages { get; }
    IHotOpRepository HotOps { get; }
    IHotOpSessionRepository HotOpSessions { get; }
    IDailyMessageCountRepository DailyMessageCounts { get; }
    ISecretSantaGroupRepository SecretSantaGroups { get; }
    IStopWordRepository StopWords { get; }
    Task<IDbContextTransaction> BeginTransaction();
    Task CommitTransaction(IDbContextTransaction transaction);
    BrobotDbContext DbContext { get; }
    Task<int> CompleteAsync();
}