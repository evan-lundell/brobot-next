using System.Text;
using Brobot.Models;
using Brobot.Repositories;
using Discord;
using Discord.Interactions;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;

namespace Brobot.Modules;


public class MusicModule : InteractionModuleBase
{
    private readonly LavaNode _node;
    private IUnitOfWork _uow;

    public MusicModule(LavaNode node, IUnitOfWork uow)
    {
        _node = node;
        _uow = uow;
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

        if (player.Vueue.Count == 0)
        {
            await RespondAsync("No songs in the queue");
            return;
        }

        var tracksBuilder = new StringBuilder();
        var index = 1;
        if (player.Track != null)
        {
            tracksBuilder.AppendLine($"1. {player.Track.Title}");
            index++;
        }
        foreach (var track in player.Vueue)
        {
            tracksBuilder.AppendLine($"{index}: {track.Title}");
            index++;
        }
        var embedBuilder = new EmbedBuilder
        {
            Title = "Queue"
        };
        embedBuilder.AddField("Tracks", tracksBuilder.ToString());
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

    [SlashCommand("playlist list", "Lists the available playlists")]
    public async Task ListPlaylists()
    {
        var playlists = await _uow.Playlists.GetAll();
        var builder = new EmbedBuilder
        {
            Color = new Color(114, 137, 218),
            Description = "Playlists"
        };

        foreach (var playlist in playlists)
        {
            builder.AddField((x) =>
            {
                x.Name = playlist.Name;
                x.Value = $"{playlist.Songs.Count} song(s)";
                x.IsInline = false;
            });
        }

        await RespondAsync(embed: builder.Build(), ephemeral: true);
    }

    [SlashCommand("playlist queue", "Queues the songs in a playlist")]
    public async Task QueuePlaylist(string name)
    {
        var playlist =
            (await _uow.Playlists.Find((p) => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            .FirstOrDefault();
        if (playlist == null)
        {
            await RespondAsync("Playlist not found", ephemeral: true);
            return;
        }
        
        if (!_node.TryGetPlayer(Context.Guild, out LavaPlayer<LavaTrack> player))
        {
            await RespondAsync(text: "I'm not connected to a voice channel", ephemeral: true);
            return;
        }

        _ = QueuePlaylistSongs(playlist, player);
        await RespondAsync($"Queuing playlist {playlist.Name}");
    }

    private async Task QueuePlaylistSongs(PlaylistModel playlistModel, LavaPlayer<LavaTrack> player)
    {
        foreach (var song in playlistModel.Songs)
        {
            var searchResult = await _node.SearchAsync(SearchType.YouTube, song.Url);
            if (searchResult.Tracks.Count > 0)
            {
                player.Vueue.Enqueue(searchResult.Tracks.First());
            }
        }
    }
}