using System.Threading.Channels;
using Brobot.Workers;
using Brobot.Tests.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Brobot.Tests.WorkersTests;

[TestFixture]
public class CronWorkerBaseTests
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

    private ObservableTimeProvider _timeProvider = null!;
    private TestWorker? _worker;

    [TearDown]
    public async Task TearDown()
    {
        if (_worker is not null)
        {
            await _worker.StopAsync(CancellationToken.None);
            _worker.Dispose();
        }
    }

    [Test]
    public async Task MonthlyCron_RunsOnTheFirstOfEachMonth_AcrossLongDelays()
    {
        // Oct 2 -> Nov 1 12:00 is 30.5 days, longer than the default 30 day MaxDelay,
        // so this also goes through the chunked-wait path that used to be broken.
        _timeProvider = new ObservableTimeProvider(Utc(2026, 10, 2));
        _worker = new TestWorker("0 12 1 * *", _timeProvider);

        await _worker.StartAsync(CancellationToken.None);
        await _timeProvider.WaitForTimerCreated();

        // First chunk elapses, worker goes back to waiting without running
        _timeProvider.SetUtcNow(Utc(2026, 11, 1, 11, 59));
        await _timeProvider.WaitForTimerCreated();
        Assert.That(_worker.Runs.Reader.TryRead(out _), Is.False, "ran before Nov 1 12:00");

        _timeProvider.SetUtcNow(Utc(2026, 11, 1, 12));
        var november = await NextRun(_worker);
        await _timeProvider.WaitForTimerCreated();

        _timeProvider.SetUtcNow(Utc(2026, 12, 1, 12));
        var december = await NextRun(_worker);

        Assert.Multiple(() =>
        {
            Assert.That(november.Target, Is.EqualTo(Utc(2026, 11, 1, 12)));
            Assert.That(november.RanAt, Is.GreaterThanOrEqualTo(november.Target));
            Assert.That(december.Target, Is.EqualTo(Utc(2026, 12, 1, 12)));
            Assert.That(december.RanAt, Is.GreaterThanOrEqualTo(december.Target));
        });
    }

    [Test]
    public async Task LongDelay_IsSplitIntoMaxDelayChunks_AndStillRunsOnTime()
    {
        _timeProvider = new ObservableTimeProvider(Utc(2026, 10, 2));
        _worker = new TestWorker("0 12 1 * *", _timeProvider, maxDelay: TimeSpan.FromDays(1));

        await _worker.StartAsync(CancellationToken.None);
        await _timeProvider.WaitForTimerCreated();
        var timersAfterStart = _timeProvider.TimersCreated;

        // Step one day at a time from Oct 2 to Nov 1; each step should fire one chunk
        // and the worker should start a new wait without running.
        for (var day = Utc(2026, 10, 3); day <= Utc(2026, 11, 1); day = day.AddDays(1))
        {
            _timeProvider.SetUtcNow(day);
            await _timeProvider.WaitForTimerCreated();
        }

        Assert.Multiple(() =>
        {
            Assert.That(_worker.Runs.Reader.TryRead(out _), Is.False, "ran before Nov 1 12:00");
            Assert.That(_timeProvider.TimersCreated - timersAfterStart, Is.EqualTo(30));
        });

        _timeProvider.SetUtcNow(Utc(2026, 11, 1, 12));
        var run = await NextRun(_worker);

        Assert.That(run.Target, Is.EqualTo(Utc(2026, 11, 1, 12)));
    }

    [Test]
    public async Task DoWorkThrowing_DoesNotStopFutureRuns()
    {
        _timeProvider = new ObservableTimeProvider(Utc(2026, 11, 13, 0, 0, 30));
        _worker = new TestWorker("* * * * *", _timeProvider);
        var calls = 0;
        _worker.OnRun = (_, _) => Interlocked.Increment(ref calls) == 1
            ? throw new InvalidOperationException("boom")
            : Task.CompletedTask;

        await _worker.StartAsync(CancellationToken.None);
        await _timeProvider.WaitForTimerCreated();

        _timeProvider.SetUtcNow(Utc(2026, 11, 13, 0, 1));
        var first = await NextRun(_worker);
        await _timeProvider.WaitForTimerCreated();

        _timeProvider.SetUtcNow(Utc(2026, 11, 13, 0, 2));
        var second = await NextRun(_worker);

        Assert.Multiple(() =>
        {
            Assert.That(first.Target, Is.EqualTo(Utc(2026, 11, 13, 0, 1)));
            Assert.That(second.Target, Is.EqualTo(Utc(2026, 11, 13, 0, 2)));
            Assert.That(calls, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task StopAsync_WhileWaiting_StopsWithoutRunning()
    {
        _timeProvider = new ObservableTimeProvider(Utc(2026, 11, 13, 0, 0, 30));
        _worker = new TestWorker("* * * * *", _timeProvider);

        await _worker.StartAsync(CancellationToken.None);
        await _timeProvider.WaitForTimerCreated();

        await _worker.StopAsync(CancellationToken.None).WaitAsync(TestTimeout);
        _timeProvider.SetUtcNow(Utc(2026, 11, 13, 0, 5));

        Assert.Multiple(() =>
        {
            Assert.That(_worker.ExecuteTask!.IsCompleted, Is.True);
            Assert.That(_worker.Runs.Reader.TryRead(out _), Is.False);
        });
    }

    [Test]
    public async Task StopAsync_DuringDoWork_CancelsWorkAndDoesNotReschedule()
    {
        _timeProvider = new ObservableTimeProvider(Utc(2026, 11, 13, 0, 0, 30));
        _worker = new TestWorker("* * * * *", _timeProvider);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var workWasCancelled = false;
        _worker.OnRun = async (_, ct) =>
        {
            started.SetResult();
            try
            {
                // Real delay, not the fake clock: blocks until the worker's token is cancelled
                await Task.Delay(Timeout.Infinite, ct);
            }
            catch (OperationCanceledException)
            {
                workWasCancelled = true;
                throw;
            }
        };

        await _worker.StartAsync(CancellationToken.None);
        await _timeProvider.WaitForTimerCreated();

        _timeProvider.SetUtcNow(Utc(2026, 11, 13, 0, 1));
        await started.Task.WaitAsync(TestTimeout);
        var timersBeforeStop = _timeProvider.TimersCreated;

        await _worker.StopAsync(CancellationToken.None).WaitAsync(TestTimeout);

        Assert.Multiple(() =>
        {
            Assert.That(workWasCancelled, Is.True);
            Assert.That(_worker.ExecuteTask!.IsCompleted, Is.True);
            Assert.That(_timeProvider.TimersCreated, Is.EqualTo(timersBeforeStop), "worker started a new wait after stopping");
        });
    }

    private static DateTimeOffset Utc(int year, int month, int day, int hour = 0, int minute = 0, int second = 0) =>
        new(year, month, day, hour, minute, second, TimeSpan.Zero);

    private static async Task<(DateTimeOffset Target, DateTimeOffset RanAt)> NextRun(TestWorker worker) =>
        await worker.Runs.Reader.ReadAsync().AsTask().WaitAsync(TestTimeout);

    private sealed class TestWorker(string cron, TimeProvider timeProvider, TimeSpan? maxDelay = null)
        : CronWorkerBase(cron, NullLogger.Instance, timeProvider)
    {
        // Field initializer rather than capturing the primary constructor parameter (CS9107)
        private readonly TimeProvider _clock = timeProvider;

        public Channel<(DateTimeOffset Target, DateTimeOffset RanAt)> Runs { get; } =
            Channel.CreateUnbounded<(DateTimeOffset Target, DateTimeOffset RanAt)>();

        public Func<DateTimeOffset, CancellationToken, Task>? OnRun { get; set; }

        protected override TimeSpan MaxDelay => maxDelay ?? base.MaxDelay;

        protected override async Task DoWork(DateTimeOffset target, CancellationToken cancellationToken)
        {
            Runs.Writer.TryWrite((target, _clock.GetUtcNow()));
            if (OnRun is not null)
            {
                await OnRun(target, cancellationToken);
            }
        }
    }
}
