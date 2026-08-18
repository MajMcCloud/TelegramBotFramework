using System;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBotBase.Tools;

/// <summary>
/// A thread-safe, lock-free gate for coordinating a shared "pause until" deadline across concurrent callers
/// </summary>
public class PauseGate
{
    private long _pausedUntilTicks;

    /// <summary>
    /// Blocks until the current pause (if any) expires. Re-checks after waiting,
    /// since pause can be extended while waiting
    /// </summary>
    public async Task WaitIfPaused(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var pausedUntil = new DateTime(Interlocked.Read(ref _pausedUntilTicks), DateTimeKind.Utc);
            var now = DateTime.UtcNow;

            if (now > pausedUntil)
            {
                return;
            }
            
            var delay = pausedUntil - now;
            await Task.Delay(delay, ct);
        }
    }

    /// <summary>
    /// Extends the pause. Never shortens it
    /// </summary>
    public void TriggerPause(TimeSpan duration)
    {
        var newPausedUntilTicks = DateTime.UtcNow.Add(duration).Ticks;

        long current;
        do
        {
            current = Interlocked.Read(ref _pausedUntilTicks);
            if (current > newPausedUntilTicks)
            {
                return;
            }
        } while (Interlocked.CompareExchange(ref _pausedUntilTicks, newPausedUntilTicks, current) != current);
    }
    
    public bool IsPaused => new DateTime(Interlocked.Read(ref _pausedUntilTicks), DateTimeKind.Utc) > DateTime.UtcNow;
}