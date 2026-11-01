using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;

namespace TelegramBotBase;

/// <summary>
///     Base class for managing all active sessions
/// </summary>
public partial class SessionManager
{
    /// <summary>
    ///     Sessions which are currently being created, used to make sure only one session gets created per device
    ///     when several updates of the same device are processed concurrently.
    /// </summary>
    private readonly ConcurrentDictionary<long, Lazy<Task<IDeviceSession>>> _pendingSessions = new();

    /// <summary>
    ///     Sessions which have been ended and are still saving or discarding their state. A new session of the same
    ///     device waits for it, so it never restores an outdated state.
    /// </summary>
    private readonly ConcurrentDictionary<long, Task> _pendingEnds = new();

    /// <summary>
    ///     Usage tracking per session. Attached to the session instance, so it goes away together with it.
    /// </summary>
    private readonly ConditionalWeakTable<IDeviceSession, SessionLease> _leases = new();

    public SessionManager(BotBase botBase)
    {
        BotBase = botBase;
        SessionList = new ConcurrentDictionary<long, IDeviceSession>();
    }

    /// <summary>
    ///     The Basic message client.
    /// </summary>
    public MessageClient Client => BotBase.Client;

    /// <summary>
    ///     A thread-safe list of all active sessions.
    /// </summary>
    public ConcurrentDictionary<long, IDeviceSession> SessionList { get; }


    /// <summary>
    ///     Reference to the Main BotBase instance for later use.
    /// </summary>
    public BotBase BotBase { get; }

    /// <summary>
    ///     Clock used for idle tracking (UTC). Can be replaced in tests.
    /// </summary>
    internal Func<DateTime> UtcNow { get; set; } = () => DateTime.UtcNow;

    /// <summary>
    ///     Get device session from Device/ChatId
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns>The session or <c>null</c> if there is no active session for this device.</returns>
    public IDeviceSession GetSession(long deviceId)
    {
        return SessionList.TryGetValue(deviceId, out var ds) ? ds : null;
    }

    /// <summary>
    ///     Start a new session with the start form. An existing session of this device gets replaced (not disposed).
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    public async Task<IDeviceSession> StartSession(long deviceId)
    {
        var ds = await CreateStartSession(deviceId);

        SessionList[deviceId] = ds;
        return ds;
    }

    /// <summary>
    ///     Returns the session of the device and acquires it for the caller, so it can't be evicted or disposed
    ///     while in use. Each returned session has to be released with <see cref="ReleaseSession" />.
    ///     A missing session gets restored from its saved state (when session limits and a state machine are
    ///     configured) or, if <paramref name="startIfMissing" /> is set, started with the start form.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="startIfMissing">Starts a new session if the device has none and no state to restore.</param>
    /// <returns>The acquired session, or <c>null</c> if there is none.</returns>
    internal async Task<AcquiredSession?> AcquireSession(long deviceId, bool startIfMissing)
    {
        while (true)
        {
            if (SessionList.TryGetValue(deviceId, out var existing))
            {
                if (TryAcquire(existing))
                {
                    return new AcquiredSession(existing, false, false);
                }

                // The session has just been ended, which removes it from the list in the same step. Look again.
                continue;
            }

            var (session, created, restored) = await GetOrCreateSession(deviceId, startIfMissing);

            if (session == null)
            {
                if (!startIfMissing)
                {
                    return null;
                }

                // A concurrent call which wasn't allowed to start a session created nothing, try again.
                continue;
            }

            // The creator already holds the lease, see GetOrCreateSession.
            if (created)
            {
                await EnforceCapacityOnStart();

                return new AcquiredSession(session, true, restored);
            }

            if (TryAcquire(session))
            {
                return new AcquiredSession(session, false, false);
            }
        }
    }

    /// <summary>
    ///     Releases a session previously acquired with <see cref="AcquireSession" />. If the session has been ended
    ///     while in use, it gets disposed now.
    /// </summary>
    internal async Task ReleaseSession(IDeviceSession session)
    {
        var lease = GetLease(session);
        bool dispose;

        lock (lease)
        {
            lease.Active--;

            dispose = lease.Ended && lease.Active == 0 && !lease.Disposed;
            if (dispose)
            {
                lease.Disposed = true;
            }
        }

        if (dispose)
        {
            await DisposeForms(session, lease.Reason == ESessionEndReason.Manual);
        }
    }

    /// <summary>
    ///     Returns the existing session of the device or creates one (restored from its saved state or started with
    ///     the start form). Concurrent calls for the same device share one creation, so only one session gets created.
    ///     When the session has been created by this call, it is returned already acquired
    ///     (see <see cref="AcquireSession" />).
    /// </summary>
    private async Task<(IDeviceSession Session, bool Created, bool Restored)> GetOrCreateSession(long deviceId,
        bool startIfMissing)
    {
        var created = false;
        var restored = false;

        var pending = _pendingSessions.GetOrAdd(deviceId, id => new Lazy<Task<IDeviceSession>>(async () =>
        {
            // A session of this device which has just been evicted could still be saving its state.
            if (_pendingEnds.TryGetValue(id, out var ending))
            {
                await ending;
            }

            // Another creation could have completed between the caller's lookup and registering this one.
            if (SessionList.TryGetValue(id, out var current))
            {
                return current;
            }

            var ds = await TryRestoreSession(id);
            var isRestored = ds != null;

            if (ds == null)
            {
                if (!startIfMissing)
                {
                    return null;
                }

                ds = await CreateStartSession(id);
            }

            // Hold the session for the creator before it becomes visible, so it can't be evicted right away.
            var lease = GetLease(ds);
            lock (lease)
            {
                lease.Active++;
            }

            if (!SessionList.TryAdd(id, ds))
            {
                // Someone added a session in the meantime (e.g. via StartSession), drop ours.
                lock (lease)
                {
                    lease.Active--;
                    lease.Ended = true;
                    lease.Disposed = true;
                }

                await DisposeForms(ds, false);

                return SessionList.TryGetValue(id, out current) ? current : null;
            }

            created = true;
            restored = isRestored;

            return ds;
        }));

        try
        {
            var session = await pending.Value;
            return (session, created, restored);
        }
        finally
        {
            // Only remove our own entry, a newer pending creation could already be registered.
            TryRemoveExact(_pendingSessions, deviceId, pending);
        }
    }

    private SessionLease GetLease(IDeviceSession session)
    {
        return _leases.GetValue(session, _ => new SessionLease());
    }

    private bool TryAcquire(IDeviceSession session)
    {
        var lease = GetLease(session);
        lock (lease)
        {
            if (lease.Ended)
            {
                return false;
            }

            lease.Active++;
            return true;
        }
    }

    /// <summary>
    ///     Removes the entry only if it still holds the given value.
    /// </summary>
    private static bool TryRemoveExact<TValue>(ConcurrentDictionary<long, TValue> dictionary, long key, TValue value)
    {
        return ((ICollection<KeyValuePair<long, TValue>>)dictionary).Remove(new KeyValuePair<long, TValue>(key, value));
    }

    private async Task<IDeviceSession> CreateStartSession(long deviceId)
    {
        var start = BotBase.FormFactory.CreateStartForm();

        start.Client = Client;

        var ds = new DeviceSession(deviceId, BotBase.RequestDispatcher, start)
        {
            LastAction = UtcNow()
        };

        start.Device = ds;
        await start.OnInit(new InitEventArgs());

        await start.OnOpened(EventArgs.Empty);

        return ds;
    }

    /// <summary>
    ///     End session. Does nothing if there is no session for this device.
    /// </summary>
    /// <param name="deviceId"></param>
    [Obsolete("Use EndSessionAsync instead.")]
    public void EndSession(long deviceId)
    {
        EndSessionAsync(deviceId).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Ends the session of the device: removes it from the session list, raises <see cref="BotBase.SessionEnds" />,
    ///     closes the active form (<c>OnClosed</c>) and disposes all forms of the session including their dependency
    ///     injection scopes. If the session is in use right now (e.g. called from within a form of this session),
    ///     closing and disposal happen once the current update has been processed.
    ///     A saved state of this device gets discarded, so the next update of the device starts a fresh session.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns><c>true</c> if an active session has been ended, otherwise <c>false</c>.</returns>
    public async Task<bool> EndSessionAsync(long deviceId)
    {
        while (SessionList.TryGetValue(deviceId, out var session))
        {
            if (await TryEndSession(deviceId, session, ESessionEndReason.Manual))
            {
                return true;
            }
        }

        // No active session, but an evicted one could still be saved.
        DiscardState(deviceId);

        return false;
    }

    /// <summary>
    ///     Ends the given session. Evictions (any reason but <see cref="ESessionEndReason.Manual" />) skip sessions
    ///     which are currently in use or for which <paramref name="canEnd" /> returns <c>false</c>.
    /// </summary>
    private async Task<bool> TryEndSession(long deviceId, IDeviceSession session, ESessionEndReason reason,
                                           Func<IDeviceSession, bool> canEnd = null)
    {
        var lease = GetLease(session);
        var ending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task pendingEnd;
        bool disposeNow;

        lock (lease)
        {
            if (lease.Ended)
            {
                return false;
            }

            if (reason != ESessionEndReason.Manual && (lease.Active > 0 || (canEnd != null && !canEnd(session))))
            {
                return false;
            }

            lease.Ended = true;
            lease.Reason = reason;

            // Registered before the session leaves the list, so a new session of this device waits for the state.
            pendingEnd = _pendingEnds.AddOrUpdate(deviceId, ending.Task,
                                                  (_, existing) => Task.WhenAll(existing, ending.Task));

            TryRemoveExact(SessionList, deviceId, session);

            disposeNow = lease.Active == 0;
            if (disposeNow)
            {
                lease.Disposed = true;
            }
        }

        try
        {
            if (reason == ESessionEndReason.Manual)
            {
                DiscardState(deviceId);
            }
            else
            {
                await PersistState(deviceId, session);
            }
        }
        finally
        {
            ending.SetResult(true);

            _ = pendingEnd.ContinueWith(_ => TryRemoveExact(_pendingEnds, deviceId, pendingEnd),
                                        TaskScheduler.Default);
        }

        try
        {
            BotBase.OnSessionEnds(new SessionEndEventArgs(deviceId, session, reason));
        }
        catch (Exception ex)
        {
            BotBase.OnException(new SystemExceptionEventArgs(null, deviceId, session, ex));
        }

        if (disposeNow)
        {
            await DisposeForms(session, reason == ESessionEndReason.Manual);
        }

        return true;
    }

    /// <summary>
    ///     Disposes all forms of the session. With <paramref name="closeActiveForm" /> the active form gets closed
    ///     first (<c>OnClosed</c>, which may send requests to Telegram); evictions skip that.
    /// </summary>
    private async Task DisposeForms(IDeviceSession session, bool closeActiveForm)
    {
        // Collect first: closing the active form clears its device and may switch the active form.
        var forms = CollectForms(session);

        if (closeActiveForm && session.ActiveForm is { IsDisposed: false } activeForm)
        {
            try
            {
                await activeForm.OnClosed(EventArgs.Empty);
            }
            catch (Exception ex)
            {
                BotBase.OnException(new SystemExceptionEventArgs(null, session.DeviceId, session, ex));
            }
        }

        foreach (var form in forms)
        {
            if (form.IsDisposed)
            {
                continue;
            }

            try
            {
                await form.DisposeAsync();
            }
            catch (Exception ex)
            {
                BotBase.OnException(new SystemExceptionEventArgs(null, session.DeviceId, session, ex));
            }
        }
    }

    /// <summary>
    ///     Returns all forms kept alive by the session: the active and previous form and every form linked to them
    ///     (see <see cref="FormBase.GetLinkedForms" />).
    /// </summary>
    internal static List<FormBase> CollectForms(IDeviceSession session)
    {
        var forms = new List<FormBase>();
        var seen = new HashSet<FormBase>();
        var queue = new Queue<FormBase>();

        queue.Enqueue(session.ActiveForm);
        queue.Enqueue(session.PreviousForm);

        while (queue.Count > 0)
        {
            var form = queue.Dequeue();
            if (form == null || !seen.Add(form))
            {
                continue;
            }

            forms.Add(form);

            foreach (var linked in form.GetLinkedForms())
            {
                queue.Enqueue(linked);
            }
        }

        return forms;
    }

    /// <summary>
    ///     Returns all active User Sessions.
    /// </summary>
    /// <returns></returns>
    public List<IDeviceSession> GetUserSessions()
    {
        return SessionList.Where(a => a.Key > 0).Select(a => a.Value).ToList();
    }

    /// <summary>
    ///     Returns all active Group Sessions.
    /// </summary>
    /// <returns></returns>
    public List<IDeviceSession> GetGroupSessions()
    {
        return SessionList.Where(a => a.Key < 0).Select(a => a.Value).ToList();
    }
}
