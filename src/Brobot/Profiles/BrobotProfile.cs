using AutoMapper;
using Brobot.Models;
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
    }
}