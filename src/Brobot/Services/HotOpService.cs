using Brobot.Dtos;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared;
using Discord;

// ReSharper disable MemberCanBeMadeStatic.Global

namespace Brobot.Services;

public class HotOpService(IUnitOfWork uow, ILogger<HotOpService> logger) : IHotOpService
{
    private const int MinuteMultiplier = 10;

    public async Task UpdateHotOps(ulong userId, UserVoiceStateAction action, IReadOnlyCollection<ulong> connectedUsers, CancellationToken cancellationToken = default)
    {
        var actionString = action == UserVoiceStateAction.Connected ? "Connected" : "Disconnected";
        using var userScope = logger.BeginScope(new Dictionary<string, object>
        {
            { "UserId", userId },
            { "Action", actionString }
        });
        logger.LogInformation("Updating active HotOps");
        var now = DateTimeOffset.UtcNow;
        var activeHotOps = await uow.HotOps.Find(ho => ho.StartDate <= now && ho.EndDate >= now, cancellationToken);
        var users = (await uow.Users.Find(u => u.Archived == false, cancellationToken)).ToDictionary(u => u.Id);

        foreach (var hotOp in activeHotOps)
        {
            using var hotOpScope = logger.BeginScope(new Dictionary<string, object>
            {
                { "HotOpId", hotOp.Id }
            });
            logger.LogInformation("Updating HotOp");
            if (hotOp.UserId == userId)
            {
                if (action == UserVoiceStateAction.Connected)
                {
                    logger.LogInformation("Creating sessions for all other users");
                    var sessions = connectedUsers
                        .Where(u => u != userId)
                        .SelectMany(u =>
                        {
                            if (!users.TryGetValue(u, out var discordUser))
                            {
                                return Enumerable.Empty<HotOpSessionModel>();
                            }

                            return
                            [
                                new HotOpSessionModel
                                {
                                    HotOp = hotOp,
                                    HotOpId = hotOp.Id,
                                    DiscordUser = discordUser,
                                    DiscordUserId = u,
                                    StartDateTime = now
                                }
                            ];
                        }
                    );
                    await uow.HotOpSessions.AddRange(sessions, cancellationToken);
                    logger.LogInformation("Finished creating sessions for all other users");
                }
                else
                {
                    logger.LogInformation("Setting endtime for all other users' sessions");
                    var existingSessions =
                        await uow.HotOpSessions.Find(hos => hos.HotOpId == hotOp.Id && hos.EndDateTime == null, cancellationToken);
                    foreach (var existingSession in existingSessions)
                    {
                        if (connectedUsers.Any(cu => cu == existingSession.DiscordUserId) && users.ContainsKey(existingSession.DiscordUserId))
                        {
                            existingSession.EndDateTime = now;
                        }
                    }
                    logger.LogInformation("Finished setting endtime for all other users' sessions");
                }
            }
            else
            {
                if (action == UserVoiceStateAction.Connected)
                {
                    // if the owner isn't in the channel, do nothing
                    if (connectedUsers.All(cu => cu != hotOp.UserId))
                    {
                        logger.LogInformation("HotOp owner is not in voice channel");
                        continue;
                    }


                    logger.LogInformation("Creating session for user");
                    if (!users.TryGetValue(userId, out var discordUser))
                    {
                        logger.LogInformation("User {UserId} not found in database", userId);
                        continue;
                    }
                    await uow.HotOpSessions.Add(new HotOpSessionModel
                    {
                        HotOp = hotOp,
                        HotOpId = hotOp.Id,
                        DiscordUser = discordUser,
                        DiscordUserId = userId,
                        StartDateTime = now
                    }, cancellationToken);
                    logger.LogInformation("Finished creating session for user");
                }
                else
                {
                    // if the owner isn't in the channel, do nothing
                    if (connectedUsers.All(cu => cu != hotOp.UserId))
                    {
                        logger.LogInformation("HotOp owner is not in voice channel");
                        continue;
                    }

                    logger.LogInformation("Setting endtime for user");
                    var session = (await uow.HotOpSessions.Find(hos =>
                            hos.HotOpId == hotOp.Id && hos.DiscordUserId == userId && hos.EndDateTime == null, cancellationToken))
                        .FirstOrDefault();
                    if (session != null)
                    {
                        session.EndDateTime = now;
                    }
                    logger.LogInformation("Finished setting endtime for user");
                }
            }
            
            logger.LogInformation("Finished updating HotOp");
        }

        await uow.CompleteAsync(cancellationToken);
        logger.LogInformation("Finished updating active HotOps");

    }

    public ScoreboardDto GetScoreboard(HotOpModel hotOp)
    {
        using var hotOpScope = logger.BeginScope(new Dictionary<string, object>
        {
            { "HotOpId", hotOp.Id }
        });
        logger.LogInformation("Getting HotOp scores");
        var scores = hotOp.Channel.ChannelUsers.Select(cu => new ScoreboardItemDto
        {
            UserId = cu.UserId,
            Username = cu.DiscordUser.Username,
            Score = 0
        })
            .Where(s => s.UserId != hotOp.UserId)
            .ToDictionary(s => s.UserId);

        foreach (var session in hotOp.HotOpSessions)
        {
            if (!scores.ContainsKey(session.DiscordUserId))
            {
                continue;
            }

            if (session.EndDateTime.HasValue)
            {
                scores[session.DiscordUserId].Score += (int)(Math.Round((session.EndDateTime.Value - session.StartDateTime).TotalMinutes, 0) * MinuteMultiplier);
            }
            else
            {
                scores[session.DiscordUserId].Score += (int)(Math.Round((DateTime.UtcNow - session.StartDateTime).TotalMinutes, 0) * MinuteMultiplier);
            }
        }

        var scoreboard = new ScoreboardDto
        {
            HotOpId = hotOp.Id,
            Scores = scores.Values.OrderByDescending(s => s.Score),
            OwnerUsername = hotOp.DiscordUser.Username
        };

        logger.LogInformation("Retrieved HotOp scoreboard");
        return scoreboard;
    }
    
    public Embed CreateScoreboardEmbed(HotOpModel hotOp)
    {
        using var hotOpScope = logger.BeginScope(new Dictionary<string, object>
        {
            { "HotOpId", hotOp.Id }
        });
        logger.LogInformation("Creating scoreboard embed");
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

        logger.LogInformation("Finished creating scoreboard embed");
        return builder.Build();
    }
}