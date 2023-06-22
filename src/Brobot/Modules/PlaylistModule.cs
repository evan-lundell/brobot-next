using System.Text;
using Brobot.Models;
using Brobot.Repositories;
using Discord;
using Discord.Interactions;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;

namespace Brobot.Modules;

[Discord.Interactions.Group("playlist", "Commands for managing playlists")]
public class PlaylistModule : InteractionModuleBase
{
    private readonly LavaNode _node;
    private readonly IUnitOfWork _uow;

    public PlaylistModule(LavaNode node, IUnitOfWork uow)
    {
        _node = node;
        _uow = uow;
    }
    
    [SlashCommand("list", "Lists the available playlists")]
    public async Task ListPlaylists()
    {
        var playlists = (await _uow.Playlists.GetAll()).ToArray();
        var builder = new EmbedBuilder
        {
            Title = "Playlists"
        };

        if (playlists.Any((p) => p.UserId == Context.User.Id))
        {
            var usersPlaylists = new StringBuilder();
            foreach (var playlist in playlists.Where((p) => p.UserId == Context.User.Id))
            {
                usersPlaylists.AppendLine($"{playlist.Name} - {playlist.Songs.Count} song(s)");
            }

            builder.AddField("Your Playlists", usersPlaylists.ToString());
        }

        if (playlists.Any((p) => p.UserId != Context.User.Id))
        {
            var otherUsersPlaylists = new StringBuilder();
            foreach (var playlist in playlists.Where((p) => p.UserId != Context.User.Id))
            {
                otherUsersPlaylists.AppendLine($"{playlist.Name} - {playlist.Songs.Count} song(s)");
            }

            builder.AddField("Other Playlists", otherUsersPlaylists.ToString());
        }

        await RespondAsync(embed: builder.Build(), ephemeral: true);
    }

    [SlashCommand("songs", "Displays the songs in a playlist")]
    public async Task ShowPlaylistSongs(string playlistName)
    {
        var playlist =
            (await _uow.Playlists.Find((p) => p.Name == playlistName))
            .FirstOrDefault();
        if (playlist == null)
        {
            await RespondAsync("Playlist not found", ephemeral: true);
            return;
        }

        var builder = new EmbedBuilder
        {
            Title = playlist.Name
        };

        var tracklistBuilder = new StringBuilder();
        var index = 1;
        foreach (var song in playlist.Songs
                     .OrderBy((s) => s.Order)
                     .Take(10))
        {
            tracklistBuilder.AppendLine($"{index}: '{song.Name}' - {song.Artist}");
            index++;
        }

        builder.AddField("Songs", tracklistBuilder.ToString());

        await RespondAsync(embed: builder.Build());
    }

    [SlashCommand("queue", "Queues the songs in a playlist")]
    public async Task QueuePlaylist(string playlistName)
    {
        var playlist =
            (await _uow.Playlists.Find((p) => p.Name == playlistName))
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
        var isFirst = player.Vueue.Count == 0;
        foreach (var song in playlistModel.Songs.OrderBy((s) => s.Order))
        {
            var searchResult = await _node.SearchAsync(SearchType.YouTube, song.Url);
            if (searchResult.Tracks.Count <= 0)
            {
                continue;
            }
            
            var track = searchResult.Tracks.First();
            if (player.PlayerState is PlayerState.Stopped or PlayerState.None && isFirst)
            {
                await player.PlayAsync(track);
                isFirst = false;
            }
            else
            {
                player.Vueue.Enqueue(track);
            }
        }
    }
}