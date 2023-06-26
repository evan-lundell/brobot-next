using AutoMapper;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlaylistsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<PlaylistsController> _logger;
    private readonly SongDataService _songDataService;
    private readonly IConfiguration _configuration;

    public PlaylistsController(
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<PlaylistsController> logger,
        SongDataService songDataService,
        IConfiguration configuration)
    {
        _logger = logger;
        _songDataService = songDataService;
        _configuration = configuration;
        _uow = uow;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlaylistResponse>>> GetPlaylists()
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        var playlists = await _uow.Playlists.GetPlaylistsFromUser(discordUser.Id);
        return Ok(_mapper.Map<IEnumerable<PlaylistResponse>>(playlists));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlaylistResponse>> GetPlaylist(int id)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        var playlistModel = await _uow.Playlists.GetById(id);
        if (playlistModel == null)
        {
            return NotFound();
        }

        if (playlistModel.UserId != discordUser.Id)
        {
            return Unauthorized();
        }

        return Ok(_mapper.Map<PlaylistResponse>(playlistModel));
    }

    [HttpGet("{playlistId}/songs")]
    public async Task<ActionResult<IEnumerable<PlaylistSongResponse>>> GetSongsFromPlaylist(int playlistId)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        var playlistModel = await _uow.Playlists.GetById(playlistId);
        if (playlistModel == null)
        {
            return NotFound();
        }

        if (playlistModel.UserId != discordUser.Id)
        {
            return Unauthorized();
        }

        return Ok(_mapper.Map<IEnumerable<PlaylistSongResponse>>(playlistModel.Songs.OrderBy((s) => s.Order)));
    }

    [HttpGet("{playlistId}/songs/{playlistSongId}")]
    public async Task<ActionResult<PlaylistSongResponse>> GetPlaylistSong(int playlistId, int playlistSongId)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        var playlistModel = await _uow.Playlists.GetById(playlistId);
        if (playlistModel == null)
        {
            return NotFound();
        }

        if (playlistModel.UserId != discordUser.Id)
        {
            return Unauthorized();
        }

        var playlistSong = await _uow.PlaylistSongs.GetById(playlistSongId);
        if (playlistSong == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<PlaylistSongResponse>(playlistSong));
    }

    [HttpPost]
    public async Task<ActionResult<PlaylistResponse>> CreatePlaylist(PlaylistRequest playlistRequest)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }   

        var playlistModel = _mapper.Map<PlaylistModel>(playlistRequest);
        playlistModel.User = discordUser;
        playlistModel.UserId = discordUser.Id;
        await _uow.Playlists.Add(playlistModel);
        await _uow.CompleteAsync();
        return Ok(_mapper.Map<PlaylistResponse>(playlistModel));
    }

    [HttpPost("{playlistId}/songs")]
    public async Task<ActionResult<PlaylistSongResponse>> CreatePlaylistSong(int playlistId, PlaylistSongRequest playlistSongRequest)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        var playlistModel = await _uow.Playlists.GetById(playlistId);
        if (playlistModel == null)
        {
            return NotFound();
        }

        if (playlistModel.UserId != discordUser.Id)
        {
            return Unauthorized();
        }

        if (!int.TryParse(_configuration["PlaylistMaxNumberOfSongs"], out int maxSongs))
        {
            maxSongs = 50;
        }

        if (playlistModel.Songs.Count == maxSongs)
        {
            return BadRequest($"Playlists cannot contain more than {maxSongs} songs");
        }
        
        var playlistSongModel = _mapper.Map<PlaylistSongModel>(playlistSongRequest);
        playlistSongModel.Playlist = playlistModel;
        playlistSongModel.PlaylistId = playlistModel.Id;

        await _uow.PlaylistSongs.Add(playlistSongModel);
        await _uow.CompleteAsync();
        return Ok(_mapper.Map<PlaylistSongResponse>(playlistSongModel));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PlaylistResponse>> UpdatePlaylist(int id, PlaylistRequest playlistRequest)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        var existingPlaylist = await _uow.Playlists.GetById(id);
        if (existingPlaylist == null)
        {
            return NotFound("Playlist not found");
        }

        if (existingPlaylist.UserId != discordUser.Id)
        {
            return Unauthorized();
        }

        existingPlaylist.Name = playlistRequest.Name;
        await _uow.CompleteAsync();
        return Ok(_mapper.Map<PlaylistResponse>(existingPlaylist));
    }

    [HttpPut("{playlistId}/song/{songId}")]
    public async Task<ActionResult<PlaylistSongResponse>> UpdatePlaylistSong(
        int playlistId, 
        int songId,
        PlaylistSongRequest playlistSongRequest)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }
        
        var playlistModel = await _uow.Playlists.GetById(playlistId);
        if (playlistModel == null)
        {
            return NotFound();
        }

        var existingSong = playlistModel.Songs.FirstOrDefault((s) => s.Id == songId);
        if (existingSong == null)
        {
            return NotFound("Playlist song not found");
        }

        if (playlistModel.UserId != discordUser.Id)
        {
            return Unauthorized();
        }
        
        if (existingSong.PlaylistId != playlistId || existingSong.Id != playlistSongRequest.Id)
        {
            return BadRequest();
        }

        await using (var transaction = await _uow.BeginTransaction())
        {
            if (existingSong.Order != playlistSongRequest.Order)
            {
                var existingOrder = existingSong.Order;
                existingSong.Order = -1;
                await _uow.CompleteAsync();

                if (existingOrder < playlistSongRequest.Order)
                {
                    foreach (var song in playlistModel.Songs.Where((s) =>
                                     s.Order > existingOrder && s.Order <= playlistSongRequest.Order)
                                 .OrderBy((s) => s.Order))
                    {
                        song.Order--;
                        await _uow.CompleteAsync();
                    }
                }
                else
                {
                    foreach (var song in playlistModel.Songs.Where((s) =>
                                     s.Order < existingOrder && s.Order >= playlistSongRequest.Order)
                                 .OrderByDescending((s) => s.Order))
                    {
                        song.Order++;
                        await _uow.CompleteAsync();
                    }
                }
            }

            existingSong.Name = playlistSongRequest.Name;
            existingSong.Order = playlistSongRequest.Order;
            existingSong.Artist = playlistSongRequest.Artist;
            existingSong.Url = playlistSongRequest.Url;

            await _uow.CompleteAsync();
            await _uow.CommitTransaction(transaction);
        }

        return Ok(_mapper.Map<PlaylistSongResponse>(existingSong));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlaylist(int id)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        var existingPlaylist = await _uow.Playlists.GetById(id);
        if (existingPlaylist == null)
        {
            return NotFound("Playlist not found");
        }

        if (existingPlaylist.UserId != discordUser.Id)
        {
            return Unauthorized();
        }
        
        _uow.Playlists.Remove(existingPlaylist);
        await _uow.CompleteAsync();
        return Ok();
    }

    [HttpDelete("{playlistId}/songs/{playlistSongId}")]
    public async Task<IActionResult> DeletePlaylistSong(int playlistId, int playlistSongId)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        var existingPlaylist = await _uow.Playlists.GetById(playlistId);
        if (existingPlaylist == null)
        {
            return NotFound("Playlist not found");
        }

        if (existingPlaylist.UserId != discordUser.Id)
        {
            return Unauthorized();
        }

        var existingSong = await _uow.PlaylistSongs.GetById(playlistSongId);
        if (existingSong == null)
        {
            return NotFound("Song not found");
        }
        
        _uow.PlaylistSongs.Remove(existingSong);
        await _uow.CompleteAsync();
        return Ok();
    }

    [HttpGet("song-data")]
    public async Task<ActionResult<SongDataResponse>> GetSongData([FromQuery] string url)
    {
        var songData = await _songDataService.GetSongData(url);
        return Ok(songData);
    }
}
