namespace Brobot.Tests.HotOpServiceTests;

/*
 * Seed data used (from HotOpServiceTestsBase):
 * - HotOp 4: owner User 2, no sessions
 * - HotOp 1: owner User 1, sessions: User 2 (closed x2), User 3 (closed), User 3 (open)
 */
[TestFixture]
public class CreateSessionsTests : HotOpServiceTestsBase
{
    private static readonly DateTimeOffset StartDateTime = new(2026, 11, 13, 20, 0, 0, TimeSpan.Zero);

    [Test]
    public async Task CreatesOpenSessionsWithGivenStartTime()
    {
        var hotOp = await GetHotOp(4);

        await HotOpService.CreateSessions(hotOp, [1UL, 3UL], StartDateTime);

        var sessions = (await UnitOfWork.HotOpSessions.Find(s => s.HotOpId == 4)).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(sessions.Select(s => s.DiscordUserId), Is.EquivalentTo(new[] { 1UL, 3UL }));
            Assert.That(sessions.All(s => s.StartDateTime == StartDateTime), Is.True);
            Assert.That(sessions.All(s => s.EndDateTime == null), Is.True);
        });
    }

    [Test]
    public async Task ExcludesOwner()
    {
        var hotOp = await GetHotOp(4);

        await HotOpService.CreateSessions(hotOp, [1UL, 2UL, 3UL], StartDateTime);

        var sessions = await UnitOfWork.HotOpSessions.Find(s => s.HotOpId == 4);
        Assert.That(sessions.Select(s => s.DiscordUserId), Is.EquivalentTo(new[] { 1UL, 3UL }));
    }

    [Test]
    public async Task SkipsUsersWithOpenSession_ButNotUsersWithOnlyClosedSessions()
    {
        // User 1 is the owner, User 3 already has an open session, User 2 only has closed ones
        var hotOp = await GetHotOp(1);

        await HotOpService.CreateSessions(hotOp, [1UL, 2UL, 3UL], StartDateTime);

        var newSessions = (await UnitOfWork.HotOpSessions.Find(s => s.HotOpId == 1 && s.StartDateTime == StartDateTime)).ToList();
        var openSessions = (await UnitOfWork.HotOpSessions.Find(s => s.HotOpId == 1 && s.EndDateTime == null)).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(newSessions.Select(s => s.DiscordUserId), Is.EquivalentTo(new[] { 2UL }));
            Assert.That(openSessions.Count(s => s.DiscordUserId == 3UL), Is.EqualTo(1), "User 3 got a duplicate open session");
        });
    }

    [Test]
    public async Task SkipsUsersWhoseOpenSessionWasCreatedAfterTheHotOpWasLoaded()
    {
        // The worker loads the hot op, then UpdateHotOps (another DbContext) opens a session for User 1
        // before CreateSessions runs. The loaded model doesn't contain that session; the database does.
        var hotOp = await GetHotOp(4);
        await AddOpenSessionFromAnotherContext(4, 1UL, StartDateTime.AddSeconds(1));
        Assert.That(hotOp.HotOpSessions, Is.Empty, "precondition: the loaded model should be stale");

        await HotOpService.CreateSessions(hotOp, [1UL, 3UL], StartDateTime);

        var openSessions = (await UnitOfWork.HotOpSessions.Find(s => s.HotOpId == 4 && s.EndDateTime == null)).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(openSessions.Count(s => s.DiscordUserId == 1UL), Is.EqualTo(1), "User 1 got a duplicate open session");
            Assert.That(openSessions.Count(s => s.DiscordUserId == 3UL), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task SkipsArchivedUsers()
    {
        var user3 = await UnitOfWork.Users.GetById(3UL);
        user3!.Archived = true;
        await UnitOfWork.CompleteAsync();
        var hotOp = await GetHotOp(4);

        await HotOpService.CreateSessions(hotOp, [1UL, 3UL], StartDateTime);

        var sessions = await UnitOfWork.HotOpSessions.Find(s => s.HotOpId == 4);
        Assert.That(sessions.Select(s => s.DiscordUserId), Is.EquivalentTo(new[] { 1UL }));
    }

    [Test]
    public async Task SkipsUsersNotInDatabase()
    {
        // e.g. bots, or users the bot hasn't synced yet
        var hotOp = await GetHotOp(4);

        await HotOpService.CreateSessions(hotOp, [1UL, 999UL], StartDateTime);

        var sessions = await UnitOfWork.HotOpSessions.Find(s => s.HotOpId == 4);
        Assert.That(sessions.Select(s => s.DiscordUserId), Is.EquivalentTo(new[] { 1UL }));
    }

    [Test]
    public async Task NoConnectedUsers_CreatesNothing()
    {
        var hotOp = await GetHotOp(4);

        await HotOpService.CreateSessions(hotOp, [], StartDateTime);

        var sessions = await UnitOfWork.HotOpSessions.Find(s => s.HotOpId == 4);
        Assert.That(sessions, Is.Empty);
    }
}
