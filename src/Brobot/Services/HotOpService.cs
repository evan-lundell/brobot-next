using Brobot.Dtos;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared;
using Discord;

// ReSharper disable MemberCanBeMadeStatic.Global

namespace Brobot.Services;

public class HotOpService(IUnitOfWork uow, ILogger<HotOpService> logger)
{
    private const int MinuteMultiplier = 10;

    public async Task UpdateHotOps(ulong userId, UserVoiceStateAction action, IReadOnlyCollection<ulong> connectedUsers)
    {
        try
        {
            var now = DateTimeOffset.UtcNow;
            var activeHotOps = await uow.HotOps.Find(ho => ho.StartDate <= now && ho.EndDate >= now);
            var users = (await uow.Users.Find(u => u.Archived == false)).ToDictionary(u => u.Id);

            foreach (var hotOp in activeHotOps)
            {
                if (hotOp.UserId == userId)
                {
                    if (action == UserVoiceStateAction.Connected)
                    {
                        var sessions = connectedUsers
                            .Where(u => u != userId)
                            .Select(u => new HotOpSessionModel
                            {
                                HotOp = hotOp,
                                HotOpId = hotOp.Id,
                                User = users[u],
                                UserId = u,
                                StartDateTime = now
                            });
                        await uow.HotOpSessions.AddRange(sessions);
                    }
                    else
                    {
                        var existingSessions =
                            await uow.HotOpSessions.Find(hos => hos.HotOpId == hotOp.Id && hos.EndDateTime == null);
                        foreach (var existingSession in existingSessions)
                        {
                            if (connectedUsers.Any(cu => cu == existingSession.UserId))
                            {
                                existingSession.EndDateTime = now;
                            }
                        }
                    }
                }
                else
                {
                    if (action == UserVoiceStateAction.Connected)
                    {
                        // if the owner isn't in the channel, do nothing
                        if (connectedUsers.All(cu => cu != hotOp.UserId))
                        {
                            continue;
                        }

                        await uow.HotOpSessions.Add(new HotOpSessionModel
                        {
                            HotOp = hotOp,
                            HotOpId = hotOp.Id,
                            User = users[userId],
                            UserId = userId,
                            StartDateTime = now
                        });
                    }
                    else
                    {
                        // if the owner isn't in the channel, do nothing
                        if (connectedUsers.All(cu => cu != hotOp.UserId))
                        {
                            continue;
                        }

                        var session = (await uow.HotOpSessions.Find(hos =>
                                hos.HotOpId == hotOp.Id && hos.UserId == userId && hos.EndDateTime == null))
                            .FirstOrDefault();
                        if (session != null)
                        {
                            session.EndDateTime = now;
                        }
                    }
                }
            }

            await uow.CompleteAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while updating hot-ops, {ExMessage}", ex.Message);
        }
    }

    public ScoreboardDto GetScoreboard(HotOpModel hotOp)
    {
        var scores = hotOp.Channel.ChannelUsers.Select(cu => new ScoreboardItemDto
        {
            UserId = cu.UserId,
            Username = cu.User.Username,
            Score = 0
        })
            .Where(s => s.UserId != hotOp.UserId)
            .ToDictionary(s => s.UserId);

        foreach (var session in hotOp.HotOpSessions)
        {
            if (!scores.ContainsKey(session.UserId))
            {
                continue;
            }

            if (session.EndDateTime.HasValue)
            {
                scores[session.UserId].Score += (int)(Math.Round((session.EndDateTime.Value - session.StartDateTime).TotalMinutes, 0) * MinuteMultiplier);
            }
            else
            {
                scores[session.UserId].Score += (int)(Math.Round((DateTime.UtcNow - session.StartDateTime).TotalMinutes, 0) * MinuteMultiplier);
            }
        }

        var scoreboard = new ScoreboardDto
        {
            HotOpId = hotOp.Id,
            Scores = scores.Values.OrderByDescending(s => s.Score),
            OwnerUsername = hotOp.User.Username
        };

        return scoreboard;
    }
    
    public Embed CreateScoreboardEmbed(HotOpModel hotOp)
    {
        var scoreboard = GetScoreboard(hotOp);
        var builder = new EmbedBuilder
        {
            Color = new Color(114, 137, 218),
            Description = $"Operation Hot {scoreboard.OwnerUsername}"

        };

        foreach (var score in scoreboard.Scores)
        {
            builder.AddField(x =>
            {
                x.Name = score.Username;
                x.Value = score.Score;
                x.IsInline = false;
            });
        }

        return builder.Build();
    }
}