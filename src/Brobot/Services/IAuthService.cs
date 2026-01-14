using Brobot.Dtos;

namespace Brobot.Services;

public interface IAuthService
{
    Task<AuthResultDto> GetOrCreateApplicationUserAsync(ulong discordUserId);
}

