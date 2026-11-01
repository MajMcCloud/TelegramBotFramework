using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase;

public partial class SessionManager
{
    /// <summary>
    ///     Makes sure only one eviction pass runs at a time.
    /// </summary>
    private readonly SemaphoreSlim _evictionLock = new(1, 1);

    private readonly object _sweeperSync = new();

    private CancellationTokenSource _sweeperCancellation;

    private Task _sweeper;

    /// <summary>
    ///     Evicts all sessions exceeding the configured <see cref="BotBase.SessionLimits" /> now: first idle
    ///     sessions, then the least recently active ones while there are more than allowed.
    ///     This runs periodically in the background while the bot is started, but can be invoked manually too.
    /// </summary>
    /// <returns>The number of evicted sessions.</returns>
    public async Task<int> EvictSessions()
    {
        var limits = BotBase.SessionLimits;
        if (limits?.IsEnabled != true)
        {
            return 0;
        }

        await _evictionLock.WaitAsync();
        try
        {
            var evicted = 0;

            if (limits.IdleTimeout is { } idleTimeout)
            {
                evicted += await EvictIdleSessions(idleTimeout);
            }

            if (limits.MaxSessions is { } maxSessions)
            {
                evicted += await EvictExceedingSessions(maxSessions);
            }

            return evicted;
        }
        finally
        {
            _evictionLock.Release();
        }
    }

    private async Task<int> EvictIdleSessions(TimeSpan idleTimeout)
    {
        var now = UtcNow();
        var evicted = 0;

        bool IsIdle(IDeviceSession session)
        {
            return now - session.LastAction > idleTimeout;
        }

        foreach (var entry in SessionList.ToArray())
        {
            if (!IsIdle(entry.Value))
            {
                continue;
            }

            if (await TryEndSession(entry.Key, entry.Value, ESessionEndReason.Idle,
                                    s => IsIdle(s) && CanEvict(s)))
            {
                evicted++;
            }
        }

        return evicted;
    }

    private async Task<int> EvictExceedingSessions(int maxSessions)
    {
        if (SessionList.Count <= maxSessions)
        {
            return 0;
        }

        var evicted = 0;

        var candidates = SessionList.ToArray()
                                    .Where(a => CanEvict(a.Value))
                                    .OrderBy(a => a.Value.LastAction)
                                    .ToList();

        foreach (var entry in candidates)
        {
            if (SessionList.Count <= maxSessions)
            {
                break;
            }

            if (await TryEndSession(entry.Key, entry.Value, ESessionEndReason.Capacity, CanEvict))
            {
                evicted++;
            }
        }

        return evicted;
    }

    /// <summary>
    ///     Keeps the session count within the limit right when a new session got created, instead of waiting for the
    ///     next sweep. Skipped if an eviction pass is already running.
    /// </summary>
    private async Task EnforceCapacityOnStart()
    {
        if (BotBase.SessionLimits?.MaxSessions is not { } maxSessions || SessionList.Count <= maxSessions)
        {
            return;
        }

        if (!await _evictionLock.WaitAsync(0))
        {
            return;
        }

        try
        {
            await EvictExceedingSessions(maxSessions);
        }
        catch (Exception ex)
        {
            BotBase.OnException(new SystemExceptionEventArgs(null, 0, null, ex));
        }
        finally
        {
            _evictionLock.Release();
        }
    }

    /// <summary>
    ///     Returns if the session may be evicted: none of its forms is marked with <see cref="KeepAlive" /> or
    ///     returns <c>false</c> for <see cref="FormBase.CanEvict" />.
    /// </summary>
    private static bool CanEvict(IDeviceSession session)
    {
        try
        {
            foreach (var form in CollectForms(session))
            {
                if (form.IsDisposed)
                {
                    continue;
                }

                if (form.GetType().GetCustomAttributes(typeof(KeepAlive), true).Length != 0 || !form.CanEvict)
                {
                    return false;
                }
            }
        }
        catch
        {
            // A failing opt-out check keeps the session, losing it would be worse.
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Starts the background sweeper if session limits are configured.
    /// </summary>
    internal void StartSweeper()
    {
        var limits = BotBase.SessionLimits;
        if (limits?.IsEnabled != true)
        {
            return;
        }

        lock (_sweeperSync)
        {
            if (_sweeper != null)
            {
                return;
            }

            _sweeperCancellation = new CancellationTokenSource();
            var token = _sweeperCancellation.Token;

            _sweeper = Task.Run(() => RunSweeper(limits.SweepInterval, token));
        }
    }

    /// <summary>
    ///     Stops the background sweeper and waits until a running eviction pass has finished.
    /// </summary>
    internal async Task StopSweeper()
    {
        Task sweeper;
        CancellationTokenSource cancellation;

        lock (_sweeperSync)
        {
            sweeper = _sweeper;
            cancellation = _sweeperCancellation;

            _sweeper = null;
            _sweeperCancellation = null;
        }

        if (sweeper == null)
        {
            return;
        }

        cancellation.Cancel();

        await sweeper;

        cancellation.Dispose();
    }

    private async Task RunSweeper(TimeSpan interval, CancellationToken token)
    {
        while (true)
        {
            try
            {
                await Task.Delay(interval, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                await EvictSessions();
            }
            catch (Exception ex)
            {
                BotBase.OnException(new SystemExceptionEventArgs(null, 0, null, ex));
            }
        }
    }
}
