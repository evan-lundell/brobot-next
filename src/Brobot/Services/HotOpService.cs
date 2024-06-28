using Brobot.Dtos;
using Brobot.Models;
using Brobot.Repositories;
using Discord;
using Discord.WebSocket;

// ReSharper disable MemberCanBeMadeStatic.Global

namespace Brobot.Services;

public class HotOpService
{
    private const int MinuteMultiplier = 10;

    private readonly IServiceProvider _services;
    public HotOpService(IServiceProvider services)
    {
        _services = services;
    }

    public async Task UserVoiceStateUpdated(
        SocketUser socketUser,
        SocketVoiceState previousVoiceState,
        SocketVoiceState currentVoiceState
    )
    {
        if ((previousVoiceState.VoiceChannel == null && currentVoiceState.VoiceChannel == null)
            || socketUser.IsBot)
        {
            return;
        }

        using var scope = _services.CreateScope();
        var utcNow = DateTime.UtcNow;
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var now = DateTime.UtcNow;
        var activeHotOps = await uow.HotOps.Find(ho => ho.StartDate <= now && ho.EndDate >= now);
        var users = (await uow.Users.Find(u => u.Archived == false)).ToDictionary(u => u.Id);

        foreach (var hotOp in activeHotOps)
        {
            // if the user that connected is the hot op owner
            if (hotOp.UserId == socketUser.Id)
            {
                // if the hot op owner joined a voice channel, create session for everyone else in the channel
                if (currentVoiceState.VoiceChannel != null)
                {
                    var sessions = currentVoiceState.VoiceChannel.ConnectedUsers
                        .Where(u => u.IsBot == false && u.Id != socketUser.Id)
                        .Select(u => new HotOpSessionModel
                        {
                            HotOp = hotOp,
                            HotOpId = hotOp.Id,
                            User = users[u.Id],
                            UserId = u.Id,
                            StartDateTime = utcNow
                        });
                    await uow.HotOpSessions.AddRange(sessions);
                }
                // if the hot op owner left a voice channel, add end date to each session
                if (previousVoiceState.VoiceChannel != null)
                {
                    var existingSessions = await uow.HotOpSessions.Find(hos => hos.HotOpId == hotOp.Id && hos.EndDateTime == null);
                    foreach (var existingSession in existingSessions)
                    {
                        if (previousVoiceState.VoiceChannel.ConnectedUsers.Any(cu => cu.Id == existingSession.UserId))
                        {
                            existingSession.EndDateTime = utcNow;
                        }
                    }
                }
            }
            // user that connect is not the hot op owner
            else
            {
                // if the user joined the channel
                if (currentVoiceState.VoiceChannel != null)
                {
                    // if the owner isn't in the channel, do nothing
                    if (currentVoiceState.VoiceChannel.ConnectedUsers.All(cu => cu.Id != hotOp.UserId))
                    {
                        continue;
                    }

                    await uow.HotOpSessions.Add(new HotOpSessionModel
                    {
                        HotOp = hotOp,
                        HotOpId = hotOp.Id,
                        User = users[socketUser.Id],
                        UserId = socketUser.Id,
                        StartDateTime = utcNow
                    });
                }
                
                // if the user left the channel
                if (previousVoiceState.VoiceChannel != null)
                {
                    // if the owner isn't in the channel, do nothing
                    if (previousVoiceState.VoiceChannel.ConnectedUsers.All(cu => cu.Id != hotOp.UserId))
                    {
                        continue;
                    }

                    var session = (await uow.HotOpSessions.Find(hos => hos.HotOpId == hotOp.Id && hos.UserId == socketUser.Id && hos.EndDateTime == null)).FirstOrDefault();
                    if (session != null)
                    {
                        session.EndDateTime = utcNow;
                    }
                }
            }
        }

        await uow.CompleteAsync();
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