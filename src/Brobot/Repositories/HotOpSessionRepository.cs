using Brobot.Contexts;
using Brobot.Models;

namespace Brobot.Repositories;

public class HotOpSessionRepository(BrobotDbContext context)
    : RepositoryBase<HotOpSessionModel, int>(context), IHotOpSessionRepository;