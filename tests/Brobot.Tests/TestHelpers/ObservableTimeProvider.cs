using Microsoft.Extensions.Time.Testing;

namespace Brobot.Tests.TestHelpers;

/// <summary>
/// FakeTimeProvider that signals each time a timer is created. Code that waits with
/// Task.Delay(..., timeProvider, ...) creates exactly one timer per delay, so tests can wait until
/// the code under test is parked in a delay before moving the clock, instead of racing it.
/// </summary>
internal sealed class ObservableTimeProvider(DateTimeOffset start) : FakeTimeProvider(start)
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

    private readonly SemaphoreSlim _timerCreated = new(0);
    private int _timersCreated;

    public int TimersCreated => Volatile.Read(ref _timersCreated);

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = base.CreateTimer(callback, state, dueTime, period);
        Interlocked.Increment(ref _timersCreated);
        _timerCreated.Release();
        return timer;
    }

    /// <summary>Waits for the next timer to be created (i.e. the code under test started a delay).</summary>
    public async Task WaitForTimerCreated()
    {
        if (!await _timerCreated.WaitAsync(WaitTimeout))
        {
            Assert.Fail("Timed out waiting for a timer to be created");
        }
    }
}
