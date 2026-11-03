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

public partial class SessionManager
{
    /// <summary>
    ///     States of sessions which are not in memory (evicted or not restored yet), used with state machines not
    ///     implementing <see cref="IDeviceStateMachine" />. They are restored on the next update of the device and
    ///     merged into <see cref="SaveSessionStates(IStateMachine)" />.
    /// </summary>
    private readonly ConcurrentDictionary<long, StateEntry> _dormantStates = new();

    /// <summary>
    ///     Number of saved states waiting in memory to be restored on the next update of their device.
    /// </summary>
    internal int DormantStateCount => _dormantStates.Count;

    /// <summary>
    ///     States get restored per device on demand, which requires session limits and a state machine.
    /// </summary>
    private bool UsesLazyStates => BotBase.StateMachine != null && BotBase.SessionLimits?.IsEnabled == true;

    private bool UsesDeviceStates(out IDeviceStateMachine machine)
    {
        machine = BotBase.StateMachine as IDeviceStateMachine;
        return machine != null && UsesLazyStates;
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
    ///     Loads the saved states when the bot starts. Without session limits all sessions are restored right away.
    ///     With session limits they are restored on the first update of each device: states of an
    ///     <see cref="IDeviceStateMachine" /> are loaded on demand, other state machines are read once and kept as
    ///     (lightweight) dormant states.
    /// </summary>
    internal async Task LoadSessionStatesOnStart()
    {
        var machine = BotBase.StateMachine;
        if (machine == null)
        {
            return;
        }

        if (!UsesLazyStates)
        {
            await LoadSessionStates(machine);
            return;
        }

        if (machine is IDeviceStateMachine)
        {
            return;
        }

        var container = machine.LoadFormStates();

        foreach (var state in container.States)
        {
            if (state == null || SessionList.ContainsKey(state.DeviceId))
            {
                continue;
            }

            _dormantStates[state.DeviceId] = state;
        }
    }

    /// <summary>
    ///     Loads the previously saved states from the machine and restores all of their sessions.
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
            //Key already existing
            if (s == null || SessionList.ContainsKey(s.DeviceId))
            {
                continue;
            }

            var device = await RestoreSession(s, statemachine);
            if (device == null)
            {
                continue;
            }

            if (!SessionList.TryAdd(s.DeviceId, device))
            {
                await DisposeForms(device, false);
            }
        }
    }

    /// <summary>
    ///     Restores the session of the device from its saved state, if session limits are configured and there is one.
    /// </summary>
    private async Task<IDeviceSession> TryRestoreSession(long deviceId)
    {
        if (!UsesLazyStates)
        {
            return null;
        }

        var machine = BotBase.StateMachine;

        if (!_dormantStates.TryRemove(deviceId, out var state) && UsesDeviceStates(out var deviceMachine))
        {
            try
            {
                state = deviceMachine.LoadFormState(deviceId);
            }
            catch (Exception ex)
            {
                BotBase.OnException(new SystemExceptionEventArgs(null, deviceId, null, ex));
                return null;
            }
        }

        if (state == null)
        {
            return null;
        }

        return await RestoreSession(state, machine);
    }

    /// <summary>
    ///     Creates the form of a saved state and a session for it (not added to the session list).
    /// </summary>
    /// <returns>The session, or <c>null</c> if the state can't be restored.</returns>
    private async Task<IDeviceSession> RestoreSession(StateEntry s, IStateMachine statemachine)
    {
        var t = s.QualifiedName != null ? Type.GetType(s.QualifiedName) : null;
        if (t == null || !t.IsSubclassOf(typeof(FormBase)))
        {
            return null;
        }

        FormBase form;
        try
        {
            form = BotBase.FormFactory.CreateForm(t);

            if (form == null && statemachine.FallbackStateForm != null)
            {
                form = BotBase.FormFactory.CreateForm(statemachine.FallbackStateForm);
            }
        }
        catch (Exception ex)
        {
            BotBase.OnException(new SystemExceptionEventArgs(null, s.DeviceId, null, ex));
            return null;
        }

        if (form == null)
        {
            return null;
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

        try
        {
            //Is Subclass of IStateForm
            if (form is IStateForm iform)
            {
                var ls = new LoadStateEventArgs
                {
                    Values = s.Values
                };
                await iform.LoadState(ls);
            }

            await form.OnInit(new InitEventArgs());

            await form.OnOpened(EventArgs.Empty);
        }
        catch (Exception ex)
        {
            //Skip on exception
            BotBase.OnException(new SystemExceptionEventArgs(null, s.DeviceId, device, ex));

            await DisposeForms(device, false);
            return null;
        }

        return device;
    }

    /// <summary>
    ///     Saves the state of an evicted session, so it can be restored on the next update of the device.
    /// </summary>
    private async Task PersistState(long deviceId, IDeviceSession session)
    {
        var machine = BotBase.StateMachine;
        if (machine == null)
        {
            return;
        }

        try
        {
            var state = await CreateStateEntry(deviceId, session, machine);

            if (UsesDeviceStates(out var deviceMachine))
            {
                if (state == null)
                {
                    deviceMachine.RemoveFormState(deviceId);
                }
                else
                {
                    deviceMachine.SaveFormState(state);
                }

                return;
            }

            if (state == null)
            {
                _dormantStates.TryRemove(deviceId, out _);
            }
            else
            {
                _dormantStates[deviceId] = state;
            }
        }
        catch (Exception ex)
        {
            BotBase.OnException(new SystemExceptionEventArgs(null, deviceId, session, ex));
        }
    }

    /// <summary>
    ///     Discards the saved state of a device whose session has been ended manually.
    /// </summary>
    private void DiscardState(long deviceId)
    {
        _dormantStates.TryRemove(deviceId, out _);

        if (!UsesDeviceStates(out var deviceMachine))
        {
            return;
        }

        try
        {
            deviceMachine.RemoveFormState(deviceId);
        }
        catch (Exception ex)
        {
            BotBase.OnException(new SystemExceptionEventArgs(null, deviceId, null, ex));
        }
    }

    /// <summary>
    ///     Creates the state entry for the active form of the session.
    /// </summary>
    /// <returns>The entry, or <c>null</c> if the form must not be saved (<see cref="IgnoreState" /> without fallback form).</returns>
    private static async Task<StateEntry> CreateStateEntry(long deviceId, IDeviceSession session, IStateMachine statemachine)
    {
        var form = session.ActiveForm;
        if (form == null)
        {
            return null;
        }

        var se = new StateEntry
        {
            DeviceId = deviceId,
            ChatTitle = session.GetChatTitle(),
            FormUri = form.GetType().FullName,
            QualifiedName = form.GetType().AssemblyQualifiedName
        };

        //Skip classes where IgnoreState attribute is existing
        if (form.GetType().GetCustomAttributes(typeof(IgnoreState), true).Length != 0)
        {
            //Skip this form, when there is no fallback state form
            if (statemachine.FallbackStateForm == null)
            {
                return null;
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

        return se;
    }


    /// <summary>
    ///     Saves all open states into the machine. States of evicted sessions not restored yet are included.
    ///     With session limits and an <see cref="IDeviceStateMachine" /> (the configured one), only the states of the
    ///     active sessions get saved (one by one), the stored states of evicted sessions are kept.
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

            try
            {
                var se = await CreateStateEntry(s.Key, s.Value, statemachine);
                if (se != null)
                {
                    states.Add(se);
                }
            }
            catch
            {
                //Continue on error (skip this form)
            }
        }

        if (ReferenceEquals(statemachine, BotBase.StateMachine) && UsesDeviceStates(out var deviceMachine))
        {
            foreach (var state in states)
            {
                try
                {
                    deviceMachine.SaveFormState(state);
                }
                catch (Exception ex)
                {
                    BotBase.OnException(new SystemExceptionEventArgs(null, state.DeviceId, null, ex));
                }
            }

            return;
        }

        var active = new HashSet<long>(states.Select(a => a.DeviceId));

        states.AddRange(_dormantStates.Values.Where(a => !active.Contains(a.DeviceId)));

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
