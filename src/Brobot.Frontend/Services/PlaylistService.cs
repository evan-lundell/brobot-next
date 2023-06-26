using System.Net.Http.Json;
using System.Web;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Services;

public class PlaylistService : ApiServiceBase<PlaylistRequest, PlaylistResponse, int>, IPlaylistService
{
    public PlaylistService(HttpClient httpClient)
        : base("api/Playlists", "Playlist", httpClient)
    {
    }

    public async Task<PlaylistSongResponse> CreatePlaylistSong(int playlistId, PlaylistSongRequest playlistSongRequest)
    {
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/{playlistId}/songs", playlistSongRequest);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PlaylistSongResponse>() ??
                   throw new Exception("Failed to parse entity Playlist Song"); 
        }
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(errorResponse?.Title ?? "Failed to create entity Playlist Song");
    }

    public async Task<PlaylistSongResponse> UpdatePlaylistSong(int playlistId, PlaylistSongRequest playlistSongRequest)
    {
        var response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{playlistId}/songs/{playlistSongRequest.Id}", playlistSongRequest);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PlaylistSongResponse>() ??
                   throw new Exception("Failed to parse entity Playlist Song"); 
        }
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(errorResponse?.Title ?? "Failed to update entity Playlist Song");
    }

    public async Task DeletePlaylistSong(int playlistId, int playlistSongId)
    {
        var response = await HttpClient.DeleteAsync($"{BaseUrl}/{playlistId}/songs/{playlistSongId}");
        if (response.IsSuccessStatusCode)
        {
            return;
        }
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(errorResponse?.Title ?? "Failed to delete entity Playlist Song");
    }
    
    public Task<SongDataResponse?> GetSongData(string url)
        => HttpClient.GetFromJsonAsync<SongDataResponse>($"api/Playlists/song-data?url={HttpUtility.UrlEncode(url)}");
}