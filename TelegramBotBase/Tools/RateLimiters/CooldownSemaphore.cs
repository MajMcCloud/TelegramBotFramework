using System;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBotBase.Tools.RateLimiters;

public class CooldownSemaphore
{
    protected readonly int Capacity;
    protected readonly TimeSpan Cooldown;
    protected readonly SemaphoreSlim Semaphore;
    
    public CooldownSemaphore(int capacity, TimeSpan cooldown)
    {
        Capacity = capacity;
        Cooldown = cooldown;
        Semaphore = new SemaphoreSlim(capacity, capacity);
    }

    public virtual Task Wait(CancellationToken ct = default)
    {
        return Semaphore.WaitAsync(ct);
    }

    public virtual void Release()
    {
        _ = ScheduleRelease();
    }
    
    private async Task ScheduleRelease()
    {
        try
        {
            await Task.Delay(Cooldown);
        }
        finally
        {
            Semaphore?.Release();
        }
    }
}