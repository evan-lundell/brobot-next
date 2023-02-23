using Brobot.Models;
using Brobot.Repositories;
using Discord.WebSocket;

namespace Brobot.Services;

public class HotOpService
{
    IServiceProvider _services;
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
            || (previousVoiceState.VoiceChannel != null && currentVoiceState.VoiceChannel != null)
            || socketUser.IsBot)
        {
            return;
        }

        using (var scope = _services.CreateScope())
        {
            var utcNow = DateTime.UtcNow;
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var now = DateTime.UtcNow;
            var activeHotOps = await uow.HotOps.Find((ho) => ho.StartDate <= now && ho.EndDate >= now);
            var users = (await uow.Users.Find((u) => u.Archived == false)).ToDictionary((u) => u.Id);

            foreach (var hotOp in activeHotOps)
            {
                var isHotOpOwner = hotOp.UserId == socketUser.Id;
                if (isHotOpOwner && currentVoiceState.VoiceChannel != null)
                {

                    var sessions = currentVoiceState.VoiceChannel.ConnectedUsers
                        .Where((u) => u.IsBot == false && u.Id != socketUser.Id)
                        .Select((u) => new HotOpSessionModel
                        {
                            HotOp = hotOp,
                            HotOpId = hotOp.Id,
                            User = users[u.Id],
                            UserId = u.Id,
                            StartDateTime = utcNow
                        });
                    await uow.HotOpSessions.AddRange(sessions);
                }
                else if (isHotOpOwner && previousVoiceState.VoiceChannel != null)
                {
                    var existingSessions = await uow.HotOpSessions.Find((hos) => hos.HotOpId == hotOp.Id && hos.EndDateTime == null);
                    foreach (var existingSession in existingSessions)
                    {
                        existingSession.EndDateTime = utcNow;
                    }
                }
                else if (!isHotOpOwner && currentVoiceState.VoiceChannel != null)
                {
                    await uow.HotOpSessions.Add(new HotOpSessionModel
                    {
                        HotOp = hotOp,
                        HotOpId = hotOp.Id,
                        User = users[socketUser.Id],
                        UserId = socketUser.Id,
                        StartDateTime = utcNow
                    });
                }
                else if (!isHotOpOwner && previousVoiceState.VoiceChannel != null)
                {
                    var session = (await uow.HotOpSessions.Find((hos) => hos.HotOpId == hotOp.Id && hos.UserId == socketUser.Id && hos.EndDateTime == null)).First();
                    if (session != null)
                    {
                        session.EndDateTime = utcNow;
                    }
                }
            }

            await uow.CompleteAsync();
        }
    }
}