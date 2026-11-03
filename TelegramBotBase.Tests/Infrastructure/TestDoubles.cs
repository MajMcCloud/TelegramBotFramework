using System.Collections.Concurrent;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Tests.Infrastructure;

/// <summary>
///     Creates forms with their parameterless constructor and records them.
/// </summary>
internal sealed class TestFormFactory : IFormFactory
{
    private readonly object _sync = new();
    private readonly List<FormBase> _created = new();

    public TestFormFactory(Type startForm)
    {
        StartForm = startForm;
    }

    public Type StartForm { get; set; }

    public IReadOnlyList<FormBase> Created
    {
        get
        {
            lock (_sync)
            {
                return _created.ToList();
            }
        }
    }

    public FormBase CreateStartForm()
    {
        return CreateForm(StartForm);
    }

    public FormBase CreateForm(Type formType)
    {
        var form = (FormBase)Activator.CreateInstance(formType);

        lock (_sync)
        {
            _created.Add(form);
        }

        return form;
    }

    public T CreateForm<T>() where T : FormBase
    {
        return (T)CreateForm(typeof(T));
    }
}

/// <summary>
///     Message loop which only runs an optional handler, so no request to Telegram is ever made.
/// </summary>
internal sealed class TestMessageLoop : IMessageLoopFactory
{
    public Func<BotBase, IDeviceSession, UpdateResult, Task> Handler { get; set; }

    public int Invocations;

    public async Task MessageLoop(BotBase bot, IDeviceSession session, UpdateResult ur, MessageResult e)
    {
        Interlocked.Increment(ref Invocations);

        if (Handler != null)
        {
            await Handler(bot, session, ur);
        }
    }

    public UpdateType[] ConfigureUpdateTypes()
    {
        return Array.Empty<UpdateType>();
    }
}

/// <summary>
///     Form recording its lifecycle.
/// </summary>
public class TrackingForm : FormBase
{
    private int _closedCount;
    private int _initCount;

    public TrackingForm()
    {
        Init += (_, _) =>
        {
            Interlocked.Increment(ref _initCount);
            return Task.CompletedTask;
        };

        Closed += (_, _) =>
        {
            Interlocked.Increment(ref _closedCount);
            return Task.CompletedTask;
        };
    }

    public int ClosedCount => Volatile.Read(ref _closedCount);

    public int InitCount => Volatile.Read(ref _initCount);

    [SaveState]
    public string Note { get; set; }
}

/// <summary>
///     Start form with a slow initialization, widening the window for concurrent session starts.
/// </summary>
public class SlowStartForm : TrackingForm
{
    public SlowStartForm()
    {
        Init += async (_, _) => await Task.Delay(100);
    }
}

[KeepAlive]
public class KeepAliveForm : TrackingForm
{
}

public class ConditionalForm : TrackingForm
{
    public bool AllowEviction { get; set; }

    public override bool CanEvict => AllowEviction;
}

[IgnoreState]
public class IgnoredForm : TrackingForm
{
}

public class StatefulForm : TrackingForm, IStateForm
{
    public int Counter { get; set; }

    public Task LoadState(LoadStateEventArgs e)
    {
        Counter = e.GetInt("counter");
        return Task.CompletedTask;
    }

    public Task SaveState(SaveStateEventArgs e)
    {
        e.SetInt("counter", Counter);
        return Task.CompletedTask;
    }
}

public class TestModalDialog : ModalDialog
{
}

/// <summary>
///     Form which keeps another form alive via <see cref="FormBase.GetLinkedForms" />.
/// </summary>
public class LinkingForm : TrackingForm
{
    public FormBase Linked { get; set; }

    // "protected internal" only because of InternalsVisibleTo, other assemblies override it as "protected".
    protected internal override IEnumerable<FormBase> GetLinkedForms()
    {
        foreach (var form in base.GetLinkedForms())
        {
            yield return form;
        }

        if (Linked != null)
        {
            yield return Linked;
        }
    }
}

/// <summary>
///     State machine storing all states in memory (bulk API only).
/// </summary>
internal class MemoryStateMachine : IStateMachine
{
    public StateContainer Stored { get; set; } = new();

    public int BulkLoads;

    public int BulkSaves;

    public Type FallbackStateForm { get; set; }

    public void SaveFormStates(SaveStatesEventArgs e)
    {
        Interlocked.Increment(ref BulkSaves);
        Stored = e.States;
    }

    public StateContainer LoadFormStates()
    {
        Interlocked.Increment(ref BulkLoads);
        return Stored;
    }
}

/// <summary>
///     State machine storing the states per device in memory.
/// </summary>
internal sealed class MemoryDeviceStateMachine : MemoryStateMachine, IDeviceStateMachine
{
    public ConcurrentDictionary<long, StateEntry> Entries { get; } = new();

    public ConcurrentQueue<long> Saved { get; } = new();

    public ConcurrentQueue<long> Loaded { get; } = new();

    public ConcurrentQueue<long> Removed { get; } = new();

    /// <summary>
    ///     Runs before a state gets saved, e.g. to block the saving.
    /// </summary>
    public Action<StateEntry> BeforeSave { get; set; }

    public void SaveFormState(StateEntry state)
    {
        BeforeSave?.Invoke(state);

        Entries[state.DeviceId] = state;
        Saved.Enqueue(state.DeviceId);
    }

    public StateEntry LoadFormState(long deviceId)
    {
        Loaded.Enqueue(deviceId);
        return Entries.TryGetValue(deviceId, out var state) ? state : null;
    }

    public void RemoveFormState(long deviceId)
    {
        Entries.TryRemove(deviceId, out _);
        Removed.Enqueue(deviceId);
    }
}

/// <summary>
///     Scoped service recording its disposal.
/// </summary>
public sealed class ScopedService : IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
    }
}

public class ScopedForm : TrackingForm
{
    public ScopedForm(ScopedService service)
    {
        Service = service;
    }

    public ScopedService Service { get; }
}
