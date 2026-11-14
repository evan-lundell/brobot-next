using Brobot.Dtos;
using Brobot.Models;
using Brobot.Shared;
using Discord;

namespace Brobot.Services;

public interface IHotOpService
{
    Task UpdateHotOps(ulong userId, UserVoiceStateAction action, IReadOnlyCollection<ulong> connectedUsers, CancellationToken cancellationToken = default);
    ScoreboardDto GetScoreboard(HotOpModel hotOp);
    Embed CreateScoreboardEmbed(HotOpModel hotOp);
    Task CreateSessions(HotOpModel hotOpModel, IReadOnlyCollection<ulong> discordUserIds, DateTimeOffset startDateTime, CancellationToken cancellationToken = default);
}