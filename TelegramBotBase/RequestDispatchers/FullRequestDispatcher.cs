using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TelegramBotBase.Base;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Tools.RateLimiters;

namespace TelegramBotBase.RequestDispatchers;

/// <summary>
/// Extends <see cref="DefaultRequestDispatcher"/> with global, per-chat, and per-group
/// rate limiting via cached, auto-expiring cooldown semaphores
/// </summary>
public class FullRequestDispatcher : DefaultRequestDispatcher
{
    protected readonly CooldownSemaphore GlobalSemaphore;
    protected readonly IMemoryCache ChatSemaphoreCache;
    protected readonly IMemoryCache GroupSemaphoreCache;
    
    /// <summary>Initializes the base dispatcher and sets up the global, per-chat, and per-group limiters</summary>
    /// <param name="settings">Throttling and retry configuration</param>
    /// <param name="client">Telegram message client used to send requests</param>
    public FullRequestDispatcher(RequestDispatcherSettings settings, MessageClient client) : base(settings, client)
    {
        GlobalSemaphore = new CooldownSemaphore(settings.MaxRequestsPerSecond, TimeSpan.FromSeconds(1));
        GlobalSemaphore.SlotAvailable += SignalSlotAvailable;
        ChatSemaphoreCache = new MemoryCache(new MemoryCacheOptions());
        GroupSemaphoreCache = new MemoryCache(new MemoryCacheOptions());
    }

    protected override SlotAttempt TryOccupySpot(IDeviceSession ds)
    {
        var baseAttempt = base.TryOccupySpot(ds);
        if (baseAttempt != SlotAttempt.Acquired)
        {
            return baseAttempt;
        }

        if (!GlobalSemaphore.TryWait())
        {
            ConcurrencyLimiter.Release();
            return SlotAttempt.BlockedGlobally;
        }

        var chat = GetChatSemaphore(ds.DeviceId);
        if (!chat.TryWait())
        {
            GlobalSemaphore.ReleaseImmediately();
            ConcurrencyLimiter.Release();
            return SlotAttempt.BlockedForSession;
        }

        if (ds.IsGroup && !GetGroupSemaphore(ds.DeviceId).TryWait())
        {
            chat.ReleaseImmediately();
            GlobalSemaphore.ReleaseImmediately();
            ConcurrencyLimiter.Release();
            return SlotAttempt.BlockedForSession;
        }

        return SlotAttempt.Acquired;
    }

    protected override void ReleaseSpot(IDeviceSession ds)
    {
        GlobalSemaphore.Release();
        GetChatSemaphore(ds.DeviceId).Release();

        if (ds.IsGroup)
        {
            GetGroupSemaphore(ds.DeviceId).Release();
        }
        
        base.ReleaseSpot(ds);
    }
    
    protected CooldownSemaphore GetChatSemaphore(long chatId)
    {
        return ChatSemaphoreCache.GetOrCreate(chatId, entry =>
            {
                entry.SlidingExpiration = Settings.ChatLimiterCacheTimeout;
                var semaphore = new CooldownSemaphore(Settings.MaxRequestsPerSecondPerChat, TimeSpan.FromSeconds(1));
                semaphore.SlotAvailable += SignalSlotAvailable;
                return semaphore;
            }
        );
    }
    
    protected CooldownSemaphore GetGroupSemaphore(long chatId)
    {
        return GroupSemaphoreCache.GetOrCreate(chatId, entry =>
            {
                entry.SlidingExpiration = Settings.GroupLimiterCacheTimeout;
                var semaphore = new CooldownSemaphore(Settings.MaxRequestsPerMinutePerGroup, TimeSpan.FromMinutes(1));
                semaphore.SlotAvailable += SignalSlotAvailable;
                return semaphore;
            }
        );
    }
}