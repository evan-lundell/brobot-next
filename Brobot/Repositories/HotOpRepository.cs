using Brobot.Contexts;
using Brobot.Dtos;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class HotOpRepository : RepositoryBase<HotOpModel, int>, IHotOpRepository
{
    public HotOpRepository(BrobotDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<ScoreboardDto>> GetActiveHotOpScoreboards()
    {
        var utcNow = DateTime.UtcNow;
        var scoreboards = new List<ScoreboardDto>();
        var minuteMultiplier = 10;
        var activeHotOps = await _context.HotOps
            .AsNoTracking()
            .Include((ho) => ho.User)
            .Include((ho) => ho.HotOpSessions)
            .ThenInclude((hos) => hos.User)
            .Where((ho) => ho.StartDate <= utcNow && ho.EndDate > utcNow)
            .ToListAsync();

        foreach (var hotOp in activeHotOps)
        {
            var scores = new Dictionary<ulong, ScoreboardItemDto>();
            foreach (var session in hotOp.HotOpSessions)
            {
                if (scores.ContainsKey(session.UserId))
                {
                    scores.Add(session.UserId, new ScoreboardItemDto
                    {
                        UserId = session.UserId,
                        Username = session.User.Username,
                        Score = 0
                    });
                }

                if (session.EndDateTime.HasValue)
                {
                    scores[session.UserId].Score += (int)(Math.Round((session.EndDateTime.Value - session.StartDateTime).TotalMinutes, 0) * minuteMultiplier);
                }
                else
                {
                    scores[session.UserId].Score += (int)(Math.Round((utcNow - session.StartDateTime).TotalMinutes, 0) * minuteMultiplier);
                }
            }

            scoreboards.Add(new ScoreboardDto
            {
                HotOpId = hotOp.Id,
                Scores = scores.Values,
                OwnerUsername = hotOp.User.Username
            });
        }

        return scoreboards;
    }
}