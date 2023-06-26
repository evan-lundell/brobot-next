using AutoMapper;
using Brobot.Dtos;
using Brobot.Models;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Identity;

namespace Brobot.Profiles;

public class BrobotProfile : Profile
{
    public BrobotProfile()
    {
        CreateMap<ScheduledMessageModel, ScheduledMessageResponse>();
        CreateMap<ChannelModel, ChannelResponse>();
        CreateMap<UserModel, UserResponse>();
        CreateMap<DailyMessageCountModel, DailyMessageCountResponse>();
        CreateMap<IdentityUser, IdentityUserResponse>();
        CreateMap<PlaylistModel, PlaylistResponse>();
        CreateMap<PlaylistSongModel, PlaylistSongResponse>();
        CreateMap<PlaylistRequest, PlaylistModel>();
        CreateMap<PlaylistSongRequest, PlaylistSongModel>();
        CreateMap<HotOpModel, HotOpResponse>();
        CreateMap<HotOpRequest, HotOpModel>();
        CreateMap<ScoreboardDto, ScoreboardResponse>();
        CreateMap<ScoreboardItemDto, ScoreboardItemResponse>();
    }
}