using System;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBotBase.Tools.RateLimiters;

/// <summary>
/// Rate limiter that allows up to <c>capacity</c> concurrent slots, where each
/// released slot becomes available again only after a cooldown period
/// </summary>
public class CooldownSemaphore
{
    protected readonly int Capacity;
    protected readonly TimeSpan Cooldown;
    protected readonly SemaphoreSlim Semaphore;
    
    /// <param name="capacity">Maximum number of concurrently held slots</param>
    /// <param name="cooldown">Time a released slot must wait before it can be reused</param>
    public CooldownSemaphore(int capacity, TimeSpan cooldown)
    {
        Capacity = capacity;
        Cooldown = cooldown;
        Semaphore = new SemaphoreSlim(capacity, capacity);
    }

    /// <summary>
    /// Waits until a slot is available and consumes it
    /// </summary>
    /// <param name="ct">Token to cancel the wait</param>
    /// <returns>Task that completes once a slot is acquired</returns>
    public virtual Task Wait(CancellationToken ct = default)
    {
        return Semaphore.WaitAsync(ct);
    }

    /// <summary>Raised after a slot has become available again once its cooldown elapsed</summary>
    public event Action SlotAvailable;

    /// <summary>
    /// Consumes a slot only if one is available right now, without waiting
    /// </summary>
    /// <returns><c>true</c> if a slot was consumed</returns>
    public virtual bool TryWait()
    {
        return Semaphore.Wait(0);
    }

    /// <summary>
    /// Gives back a slot that was consumed but never used, skipping the cooldown
    /// </summary>
    public virtual void ReleaseImmediately()
    {
        Semaphore.Release();
    }

    /// <summary>
    /// Releases a slot after the cooldown period instead of immediately
    /// </summary>
    public virtual void Release()
    {
        _ = ScheduleRelease();
    }
    
    protected virtual async Task ScheduleRelease()
    {
        try
        {
            await Task.Delay(Cooldown);
        }
        finally
        {
            Semaphore?.Release();
            SlotAvailable?.Invoke();
        }
    }
}