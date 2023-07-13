using System.Text;
using Discord;
using Discord.Interactions;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;

namespace Brobot.Modules;


public class MusicModule : InteractionModuleBase
{
    private readonly LavaNode _node;

    public MusicModule(LavaNode node)
    {
        _node = node;
    }

    [SlashCommand("join", "Join a voice channel")]
    public async Task Join()
    {
        if (_node.HasPlayer(Context.Guild))
        {
            await RespondAsync(text: "I'm already connected to a voice channel", ephemeral: true);
            return;
        }
        if (!(Context.User is IVoiceState voiceState) || voiceState.VoiceChannel == null)
        {
            await RespondAsync(text: "You must be connected to a voice channel", ephemeral: true);
            return;
        }

        await _node.JoinAsync(voiceState.VoiceChannel);
        await RespondAsync($"Joined {voiceState.VoiceChannel.Name}!");
    }

    [SlashCommand("leave", "Leaves the current voice channel")]
    public async Task Leave()
    {
        if (!_node.TryGetPlayer(Context.Guild, out LavaPlayer<LavaTrack> player))
        {
            await RespondAsync(text: "I'm not connected to a voice channel", ephemeral: true);
            return;
        }

        await _node.LeaveAsync(player.VoiceChannel);
        await RespondAsync("Bye!");
    }

    [SlashCommand("play", "Plays a song")]
    public async Task Play(string query)
    {
        if (!_node.TryGetPlayer(Context.Guild, out LavaPlayer<LavaTrack> player))
        {
            await RespondAsync(text: "I'm not connected to a voice channel", ephemeral: true);
            return;
        }

        var searchResponse = Uri.IsWellFormedUriString(query, UriKind.Absolute)
            ? await _node.SearchAsync(SearchType.Direct, query)
            : await _node.SearchAsync(SearchType.YouTube, query);
        if (searchResponse.Status == SearchStatus.NoMatches
            || searchResponse.Status == SearchStatus.LoadFailed)
        {
            await RespondAsync(text: "Not found", ephemeral: true);
            return;
        }

        var tracks = searchResponse.Tracks.ToList();
        if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
        {
            if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
            {
                foreach (var track in tracks)
                {
                    player.Vueue.Enqueue(track);
                }

                await RespondAsync($"Queued {tracks.Count} track(s)");
            }
            else
            {
                player.Vueue.Enqueue(tracks[0]);
                await RespondAsync($"Queued ``{tracks[0].Title}``");
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(searchResponse.Playlist.Name))
            {
                for (int i = 0; i < tracks.Count; i++)
                {
                    if (i == 0)
                    {
                        await player.PlayAsync(tracks[i]);
                        await RespondAsync($"Now playing ``{tracks[i].Title}``");
                    }
                    else
                    {
                        player.Vueue.Enqueue(tracks[i]);
                    }
                }
            }
            else
            {
                await player.PlayAsync(tracks[0]);
                await RespondAsync($"Now playing {tracks[0].Title}");
            }
        }
    }

    [SlashCommand("pause", "Pauses the current song")]
    public async Task Pause()
    {
        if (!_node.TryGetPlayer(Context.Guild, out LavaPlayer<LavaTrack> player))
        {
            await RespondAsync(text: "I'm not connected to a voice channel", ephemeral: true);
            return;
        }

        if (player.PlayerState != PlayerState.Playing)
        {
            await RespondAsync(text: "No song currently playing", ephemeral: true);
            return;
        }

        await player.PauseAsync();
        await RespondAsync("Paused");
    }

    [SlashCommand("resume", "Resumes the current song")]
    public async Task Resume()
    {
        if (!_node.TryGetPlayer(Context.Guild, out LavaPlayer<LavaTrack> player))
        {
            await RespondAsync(text: "I'm not connected to a voice channel", ephemeral: true);
            return;
        }

        if (player.PlayerState != PlayerState.Paused)
        {
            await RespondAsync(text: "Song is not currently paused", ephemeral: true);
            return;
        }
        await RespondAsync($"Resuming {player.Track.Title}");
    }

    [SlashCommand("queue", "Displays the queue")]
    public async Task Queue()
    {
        if (!_node.TryGetPlayer(Context.Guild, out LavaPlayer<LavaTrack> player))
        {
            await RespondAsync(text: "I'm not connected to a voice channel", ephemeral: true);
            return;
        }

        if (player.Vueue.Count == 0 && player.Track == null)
        {
            await RespondAsync("No songs in the queue");
            return;
        }

        var embedBuilder = new EmbedBuilder
        {
            Title = "Queue"
        };
        
        if (player.Track != null)
        {
            embedBuilder.AddField("Now Playing", player.Track.Title);
        }
        
        var tracksBuilder = new StringBuilder();
        var index = 1;
        foreach (var track in player.Vueue.Take(10))
        {
            tracksBuilder.AppendLine($"{index}: {track.Title}");
            index++;
        }

        if (player.Vueue.Count > 10)
        {
            tracksBuilder.AppendLine($"{player.Vueue.Count - 10} more song(s)");
        }

        if (player.Vueue.Count > 0)
        {
            embedBuilder.AddField("Tracks", tracksBuilder.ToString());
        }

        await RespondAsync(embed: embedBuilder.Build());
    }

    [SlashCommand("skip", "Skips the current track")]
    public async Task Skip()
    {
        if (!_node.TryGetPlayer(Context.Guild, out LavaPlayer<LavaTrack> player))
        {
            await RespondAsync(text: "I'm not connected to a voice channel", ephemeral: true);
            return;
        }

        if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
        {
            await RespondAsync(text: "No track currently playing", ephemeral: true);
            return;
        }

        if (player.Vueue.Count == 0)
        {
            await RespondAsync(text: "No songs in the queue", ephemeral: true);
            return;
        }

        if (!player.Vueue.TryDequeue(out LavaTrack nextTrack))
        {
            await RespondAsync(text: "Unable to dequeue next track", ephemeral: true);
        }
        await player.PlayAsync(nextTrack);
        await RespondAsync($"Now playing ``{nextTrack.Title}``");
    }

    [SlashCommand("stop", "Stops the current song")]
    public async Task Stop()
    {
        if (!_node.TryGetPlayer(Context.Guild, out LavaPlayer<LavaTrack> player))
        {
            await RespondAsync(text: "I'm not connected to a voice channel", ephemeral: true);
            return;
        }

        if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
        {
            await RespondAsync(text: "No track currently playing", ephemeral: true);
            return;
        }

        await player.StopAsync();
        await RespondAsync("Player stopped");
    }
}