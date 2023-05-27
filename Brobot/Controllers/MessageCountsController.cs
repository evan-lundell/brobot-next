using AutoMapper;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TimeZoneConverter;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class MessageCountsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public MessageCountsController(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    [HttpGet("daily")]
    public async Task<ActionResult<IEnumerable<DailyMessageCountResponse>>> GetDailyMessageCounts([FromQuery] int numOfDays = 10)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            return Ok(Array.Empty<DailyMessageCountResponse>());
        }

        var timezone = TZConvert.GetTimeZoneInfo(discordUser.Timezone);
        var userDateTimeNow = DateTime.UtcNow + timezone.GetUtcOffset(DateTime.UtcNow);
        var currentDate = DateOnly.FromDateTime(userDateTimeNow);
        var startDate = currentDate.AddDays(numOfDays * -1);
        var counts = await _uow.DailyMessageCounts.GetByUserAndDateRange(
            discordUser.Id,
            startDate,
            currentDate);

        var userResponse = _mapper.Map<UserResponse>(discordUser);
        var countsResponse = _mapper.Map<IEnumerable<DailyMessageCountResponse>>(counts)
            .ToDictionary((dmc) => dmc.CountDate);
        var countsWithEmptyDays = new List<DailyMessageCountResponse>();
        var nextDate = startDate;
        while (nextDate <= currentDate)
        {
            if (countsResponse.TryGetValue(nextDate, out var count))
            {
                countsWithEmptyDays.Add(count);
            }
            else
            {
                countsWithEmptyDays.Add(new DailyMessageCountResponse
                {
                    User = userResponse,
                    CountDate = nextDate,
                    MessageCount = 0
                });
            }
            nextDate = nextDate.AddDays(1);
        }
        

        return Ok(_mapper.Map<IEnumerable<DailyMessageCountResponse>>(countsWithEmptyDays));
    }
}