using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;
using TelegramBotBase.Tools;

namespace TelegramBotBase;

/// <summary>
///     Base class for managing all active sessions
/// </summary>
public class SessionManager
{
    /// <summary>
    ///     Sessions which are currently being created, used to make sure only one session gets created per device
    ///     when several updates of the same device are processed concurrently.
    /// </summary>
    private readonly ConcurrentDictionary<long, Lazy<Task<IDeviceSession>>> _pendingSessions = new();

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
    ///     Start a new session
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
    ///     Returns the existing session of the device or starts a new one. Concurrent calls for the same device
    ///     share one start, so only one session (and one start form) gets created.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns>The session and if it has been created by this call.</returns>
    internal async Task<(IDeviceSession Session, bool Created)> GetOrStartSession(long deviceId)
    {
        if (SessionList.TryGetValue(deviceId, out var existing))
        {
            return (existing, false);
        }

        var created = false;
        var pending = _pendingSessions.GetOrAdd(deviceId, id => new Lazy<Task<IDeviceSession>>(async () =>
        {
            // Another start could have completed between the lookup above and registering this one.
            if (SessionList.TryGetValue(id, out var current))
            {
                return current;
            }

            created = true;

            var ds = await CreateStartSession(id);

            SessionList[id] = ds;
            return ds;
        }));

        try
        {
            var session = await pending.Value;
            return (session, created);
        }
        finally
        {
            // Only remove our own entry, a newer pending start could already be registered.
            TryRemoveExact(_pendingSessions, deviceId, pending);
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
    public void EndSession(long deviceId)
    {
        SessionList.TryRemove(deviceId, out _);
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

    /// <summary>
    ///     Loads the previously saved states from the machine.
    /// </summary>
    public async Task LoadSessionStates()
    {
        if (BotBase.StateMachine == null)
        {
            return;
        }

        await LoadSessionStates(BotBase.StateMachine);
    }


    /// <summary>
    ///     Loads the previously saved states from the machine.
    /// </summary>
    public async Task LoadSessionStates(IStateMachine statemachine)
    {
        if (statemachine == null)
        {
            throw new ArgumentNullException(nameof(statemachine),
                                            "No StateMachine defined. Please set one to property BotBase.StateMachine");
        }

        var container = statemachine.LoadFormStates();

        foreach (var s in container.States)
        {
            var t = Type.GetType(s.QualifiedName);
            if (t == null || !t.IsSubclassOf(typeof(FormBase)))
            {
                continue;
            }

            //Key already existing
            if (SessionList.ContainsKey(s.DeviceId))
            {
                continue;
            }

            var form = BotBase.FormFactory.CreateForm(t);

            if (form == null && statemachine.FallbackStateForm != null)
            {
                form = BotBase.FormFactory.CreateForm(statemachine.FallbackStateForm);
            }

            if (form == null)
            {
                continue;
            }

            if (t == form.GetType() && s.Values != null && s.Values.Count > 0)
            {
                var properties = s.Values.Where(a => a.Key.StartsWith("$"));
                var fields = form.GetType()
                                 .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                 .Where(a => a.GetCustomAttributes(typeof(SaveState), true).Length != 0).ToList();

                foreach (var p in properties)
                {
                    var f = fields.FirstOrDefault(a => a.Name == p.Key.Substring(1));
                    if (f == null)
                    {
                        continue;
                    }

                    try
                    {
                        if (f.PropertyType.IsEnum)
                        {
                            var ent = Enum.Parse(f.PropertyType, p.Value.ToString());

                            f.SetValue(form, ent);

                            continue;
                        }


                        f.SetValue(form, p.Value);
                    }
                    catch (ArgumentException)
                    {
                        Conversion.CustomConversionChecks(form, p, f);
                    }
                    catch
                    {
                    }
                }
            }

            form.Client = Client;
            var device = new DeviceSession(s.DeviceId, BotBase.RequestDispatcher, form)
            {
                ChatTitle = s.ChatTitle,
                LastAction = UtcNow()
            };

            if (!SessionList.TryAdd(s.DeviceId, device))
            {
                continue;
            }

            //Is Subclass of IStateForm
            if (form is IStateForm iform)
            {
                var ls = new LoadStateEventArgs
                {
                    Values = s.Values
                };
                await iform.LoadState(ls);
            }

            try
            {
                await form.OnInit(new InitEventArgs());

                await form.OnOpened(EventArgs.Empty);
            }
            catch
            {
                //Skip on exception
                TryRemoveExact(SessionList, s.DeviceId, (IDeviceSession)device);
            }
        }
    }


    /// <summary>
    ///     Saves all open states into the machine.
    /// </summary>
    public async Task SaveSessionStates(IStateMachine statemachine)
    {
        if (statemachine == null)
        {
            throw new ArgumentNullException(nameof(statemachine),
                                            "No StateMachine defined. Please set one to property BotBase.StateMachine");
        }

        var states = new List<StateEntry>();

        foreach (var s in SessionList)
        {
            if (s.Value == null)
            {
                continue;
            }

            var form = s.Value.ActiveForm;

            try
            {
                var se = new StateEntry
                {
                    DeviceId = s.Key,
                    ChatTitle = s.Value.GetChatTitle(),
                    FormUri = form.GetType().FullName,
                    QualifiedName = form.GetType().AssemblyQualifiedName
                };

                //Skip classes where IgnoreState attribute is existing
                if (form.GetType().GetCustomAttributes(typeof(IgnoreState), true).Length != 0)
                {
                    //Skip this form, when there is no fallback state form
                    if (statemachine.FallbackStateForm == null)
                    {
                        continue;
                    }

                    //Replace form by default State one.
                    se.FormUri = statemachine.FallbackStateForm.FullName;
                    se.QualifiedName = statemachine.FallbackStateForm.AssemblyQualifiedName;
                }

                //Is Subclass of IStateForm
                if (form is IStateForm iform)
                {
                    //Loading Session states
                    var ssea = new SaveStateEventArgs();
                    await iform.SaveState(ssea);

                    se.Values = ssea.Values;
                }

                //Search for public properties with SaveState attribute
                var fields = form.GetType()
                                 .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                 .Where(a => a.GetCustomAttributes(typeof(SaveState), true).Length != 0).ToList();

                foreach (var f in fields)
                {
                    var val = f.GetValue(form);

                    se.Values.Add("$" + f.Name, val);
                }

                states.Add(se);
            }
            catch
            {
                //Continue on error (skip this form)
            }
        }

        var sc = new StateContainer
        {
            States = states
        };

        statemachine.SaveFormStates(new SaveStatesEventArgs(sc));
    }

    /// <summary>
    ///     Saves all open states into the machine.
    /// </summary>
    public async Task SaveSessionStates()
    {
        if (BotBase.StateMachine == null)
        {
            return;
        }


        await SaveSessionStates(BotBase.StateMachine);
    }
}
