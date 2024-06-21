using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class SecretSantaGroupRepository : RepositoryBase<SecretSantaGroupModel, int>, ISecretSantaGroupRepository
{
    public SecretSantaGroupRepository(BrobotDbContext context) 
        : base(context)
    {
    }

    public override async Task<IEnumerable<SecretSantaGroupModel>> GetAll()
    {
        var secretSantaGroupModels = await Context.SecretSantaGroups
            .Include((ssg) => ssg.SecretSantaGroupUsers)
            .ThenInclude((ssgu) => ssgu.User)
            .ToListAsync();
        return secretSantaGroupModels;
    }

    public override Task<SecretSantaGroupModel?> GetById(int id) =>
        Context.SecretSantaGroups
            .Include((ssg) => ssg.SecretSantaGroupUsers)
            .ThenInclude((ssgu) => ssgu.User)
            .SingleOrDefaultAsync((ssg) => ssg.Id == id);

    public override Task<SecretSantaGroupModel?> GetByIdNoTracking(int id) =>
        Context.SecretSantaGroups
            .Include((ssg) => ssg.SecretSantaGroupUsers)
            .ThenInclude((ssgu) => ssgu.User)
            .SingleOrDefaultAsync((ssg) => ssg.Id == id);

    public async Task<IEnumerable<SecretSantaPairModel>> GetPairs(int secretSantaGroupId, int year)
    {
        var pairs = await Context.SecretSantaPairs
            .Where((ssp) => ssp.SecretSantaGroupId == secretSantaGroupId && ssp.Year == year)
            .ToListAsync();
        return pairs;
    }
}