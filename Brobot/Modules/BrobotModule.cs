using Discord.Interactions;

namespace Brobot.Modules;

public class BrobotModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("info", "Returns server and channel ids")]
    public async Task Info()
    {
        await ReplyAsync($"Server: {Context.Guild.Id}\nChannel: {Context.Channel.Id}");
    }
}