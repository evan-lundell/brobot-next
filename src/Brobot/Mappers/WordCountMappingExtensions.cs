using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class WordCountMappingExtensions
{
    public static WordCountResponse ToWordCountResponse(this WordCountModel model)
    {
        return new WordCountResponse
        {
            Word = model.Word,
            Count = model.Count,
        };
    }
}