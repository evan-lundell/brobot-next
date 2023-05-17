using AutoMapper;
using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Profiles;

public class BrobotProfile : Profile
{
    public BrobotProfile()
    {
        CreateMap<ScheduledMessageModel, ScheduledMessageResponse>();
    }
}