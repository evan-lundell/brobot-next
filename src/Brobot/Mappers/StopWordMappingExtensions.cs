using Brobot.Models;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class StopWordMappingExtensions
{
    public static StopWordResponse ToStopWordResponse(this StopWordModel model)
    {
        return new StopWordResponse
        {
            Id = model.Id,
            Word = model.Word
        };
    }

    public static StopWordModel ToStopWordModel(this StopWordRequest request)
    {
        return new StopWordModel
        {
            Word = request.Word
        };
    }
}
