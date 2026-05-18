using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class SecretSantaGroupRepository(BrobotDbContext context)
    : RepositoryBase<SecretSantaGroupModel, int>(context), ISecretSantaGroupRepository
{
    public override async Task<IEnumerable<SecretSantaGroupModel>> GetAll(CancellationToken cancellationToken = default)
    {
        var secretSantaGroupModels = await Context.SecretSantaGroups
            .Include(ssg => ssg.SecretSantaGroupUsers)
            .ThenInclude(ssgu => ssgu.DiscordUser)
            .ToListAsync(cancellationToken);
        return secretSantaGroupModels;
    }

    public override Task<SecretSantaGroupModel?> GetById(int id, CancellationToken cancellationToken = default) =>
        Context.SecretSantaGroups
            .Include(ssg => ssg.SecretSantaGroupUsers)
            .ThenInclude(ssgu => ssgu.DiscordUser)
            .SingleOrDefaultAsync(ssg => ssg.Id == id, cancellationToken);

    public override Task<SecretSantaGroupModel?> GetByIdNoTracking(int id, CancellationToken cancellationToken = default) =>
        Context.SecretSantaGroups
            .Include(ssg => ssg.SecretSantaGroupUsers)
            .ThenInclude(ssgu => ssgu.DiscordUser)
            .SingleOrDefaultAsync(ssg => ssg.Id == id, cancellationToken);

    public async Task<IEnumerable<SecretSantaPairModel>> GetPairs(int secretSantaGroupId, int year, CancellationToken cancellationToken = default)
    {
        var pairs = await Context.SecretSantaPairs
            .Where(ssp => ssp.SecretSantaGroupId == secretSantaGroupId && ssp.Year == year)
            .ToListAsync(cancellationToken);
        return pairs;
    }
}