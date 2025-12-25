using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class SecretSantaGroupRepository(BrobotDbContext context)
    : RepositoryBase<SecretSantaGroupModel, int>(context), ISecretSantaGroupRepository
{
    public override async Task<IEnumerable<SecretSantaGroupModel>> GetAll()
    {
        var secretSantaGroupModels = await Context.SecretSantaGroups
            .Include(ssg => ssg.SecretSantaGroupUsers)
            .ThenInclude(ssgu => ssgu.DiscordUser)
            .ToListAsync();
        return secretSantaGroupModels;
    }

    public override Task<SecretSantaGroupModel?> GetById(int id) =>
        Context.SecretSantaGroups
            .Include(ssg => ssg.SecretSantaGroupUsers)
            .ThenInclude(ssgu => ssgu.DiscordUser)
            .SingleOrDefaultAsync(ssg => ssg.Id == id);

    public override Task<SecretSantaGroupModel?> GetByIdNoTracking(int id) =>
        Context.SecretSantaGroups
            .Include(ssg => ssg.SecretSantaGroupUsers)
            .ThenInclude(ssgu => ssgu.DiscordUser)
            .SingleOrDefaultAsync(ssg => ssg.Id == id);

    public async Task<IEnumerable<SecretSantaPairModel>> GetPairs(int secretSantaGroupId, int year)
    {
        var pairs = await Context.SecretSantaPairs
            .Where(ssp => ssp.SecretSantaGroupId == secretSantaGroupId && ssp.Year == year)
            .ToListAsync();
        return pairs;
    }
}