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
    IPlaylistRepository Playlists { get; }
    IPlaylistSongRepository PlaylistSongs { get; }
    ISecretSantaGroupRepository SecretSantaGroups { get; }
    IStopWordRepository StopWords { get; }
    IWordCountRepository WordCounts { get; }
    Task<IDbContextTransaction> BeginTransaction();
    Task CommitTransaction(IDbContextTransaction transaction);
    Task<int> CompleteAsync();
}