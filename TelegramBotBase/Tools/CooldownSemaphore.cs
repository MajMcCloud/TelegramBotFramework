using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBotBase.Tools;

/// <summary>
/// Represents SemaphoreSlim wrapper that enforces a minimum cooldown between acquiring and releasing a permit
/// </summary>
public class CooldownSemaphore
{
    protected readonly SemaphoreSlim Semaphore;
    protected readonly ConcurrentDictionary<long, long> Timestamps = new();
    protected long CurrentId;

    /// <summary>
    /// Minimum duration that must elapse between a permit's
    /// acquisition and its actual release
    /// </summary>
    public TimeSpan Cooldown { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CooldownSemaphore"/> class
    /// </summary>
    /// <param name="maxCount">The maximum number of concurrent permits</param>
    /// <param name="cooldown">The minimum cooldown duration enforced before a permit is released</param>
    public CooldownSemaphore(int maxCount, TimeSpan cooldown)
    {
        Semaphore = new SemaphoreSlim(maxCount, maxCount);
        Cooldown = cooldown;
    }

    /// <summary>
    /// Asynchronously waits to acquire a permit
    /// </summary>
    /// <param name="ct">A token to cancel the wait operation</param>
    /// <returns>A unique identifier for the acquired permit, used later to release it</returns>
    public async Task<long> WaitAsync(CancellationToken ct = default)
    {
        await Semaphore.WaitAsync(ct);

        var id = Interlocked.Increment(ref CurrentId);
        Timestamps[id] = DateTime.UtcNow.Ticks;
        
        return id;
    }

    /// <summary>
    /// Releases a previously acquired permit, waiting out any remaining cooldown
    /// time before the permit becomes available again
    /// </summary>
    /// <param name="id">The permit identifier returned by <see cref="WaitAsync"/></param>
    /// <param name="ct">A token to cancel the delay before release</param>
    /// <returns>A task that completes once the permit has been released</returns>
    public async Task Release(long id, CancellationToken ct = default)
    {
        if (!Timestamps.Remove(id, out var timestamp)) return;

        if (Time.IsExpired(timestamp, Cooldown, out var estimated))
        {
            estimated = TimeSpan.Zero;
        }
        
        await Task.Delay(estimated, ct);
        Semaphore.Release();
    }

    /// <summary>
    /// Removes all expired permits from tracking and releases the semaphore
    /// for each one, making the corresponding slots available again
    /// </summary>
    public void ReleaseExpired()
    {
        var expired = Timestamps.Where(kvp => Time.IsExpired(kvp.Value, Cooldown, out _)).ToList();

        foreach (var kvp in expired)
        {
            Timestamps.TryRemove(kvp.Key, out _);
            Semaphore.Release();
        }
    }
}