using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class SecretSantaPairMappingExtensions
{
    public static SecretSantaPairResponse ToSecretSantaPairResponse(this SecretSantaPairModel model)
    {
        return new SecretSantaPairResponse
        {
            Giver = model.GiverDiscordUser.ToUserResponse(),
            Recipient = model.RecipientDiscordUser.ToUserResponse(),
            Year = model.Year
        };
    }
}