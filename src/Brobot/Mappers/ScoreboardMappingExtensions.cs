using Brobot.Dtos;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class ScoreboardMappingExtensions
{
    public static ScoreboardResponse ToScoreboardResponse(this ScoreboardDto dto)
    {
        return new ScoreboardResponse
        {
            HotOpId = dto.HotOpId,
            OwnerUsername = dto.OwnerUsername,
            Scores = dto.Scores.Select(s => s.ToScoreboardItemResponse()).ToList()
        };
    }

    public static ScoreboardItemResponse ToScoreboardItemResponse(this ScoreboardItemDto dto)
    {
        return new ScoreboardItemResponse
        {
            UserId = dto.UserId,
            Username = dto.Username,
            Score = dto.Score
        };
    }
} 