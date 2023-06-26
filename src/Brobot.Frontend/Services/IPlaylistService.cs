using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Services;

public interface IPlaylistService : IApiService<PlaylistRequest, PlaylistResponse, int>
{
    Task<PlaylistSongResponse> CreatePlaylistSong(int playlistId, PlaylistSongRequest playlistSongRequest);
    Task<PlaylistSongResponse> UpdatePlaylistSong(int playlistId, PlaylistSongRequest playlistSongRequest);
    Task DeletePlaylistSong(int playlistId, int playlistSongId);
    Task<SongDataResponse?> GetSongData(string url);
}