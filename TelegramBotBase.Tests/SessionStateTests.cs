using TelegramBotBase.Base;
using TelegramBotBase.Tests.Infrastructure;

namespace TelegramBotBase.Tests;

public class SessionStateTests
{
    private static readonly TimeSpan Idle = TimeSpan.FromMinutes(30);

    private static async Task Evict(TestBot bot)
    {
        bot.Advance(TimeSpan.FromDays(1));
        await bot.Sessions.EvictSessions();
    }

    private static StateEntry Entry(long deviceId, Type form, string note)
    {
        var entry = new StateEntry
        {
            DeviceId = deviceId,
            ChatTitle = "chat " + deviceId,
            FormUri = form.FullName,
            QualifiedName = form.AssemblyQualifiedName
        };
        entry.Values["$Note"] = note;
        return entry;
    }

    [Fact]
    public async Task EvictedSession_IsRestoredOnNextUpdate_WithPlainStateMachine()
    {
        var machine = new MemoryStateMachine();
        var bot = TestBot.Create(typeof(StatefulForm), machine, l => l.IdleTimeout = Idle);

        await bot.Send(1);
        var first = bot.ActiveForm<StatefulForm>(1);
        first.Note = "kept";
        first.Counter = 41;

        await Evict(bot);

        Assert.Null(bot.Sessions.GetSession(1));
        Assert.True(first.IsDisposed);
        Assert.Equal(1, bot.Sessions.DormantStateCount);

        await bot.Send(1);

        var restored = bot.ActiveForm<StatefulForm>(1);
        Assert.NotSame(first, restored);
        Assert.Equal("kept", restored.Note);
        Assert.Equal(41, restored.Counter);
        Assert.Equal(1, restored.InitCount);
        Assert.NotNull(bot.Sessions.GetSession(1).LastMessage);
        Assert.Equal(0, bot.Sessions.DormantStateCount);

        // A restored session continues, it doesn't begin again.
        Assert.Single(bot.Began);
        Assert.Empty(bot.Errors);
    }

    [Fact]
    public async Task EvictedSession_IsSavedAndRestoredPerDevice_WithDeviceStateMachine()
    {
        var machine = new MemoryDeviceStateMachine();
        var bot = TestBot.Create(typeof(StatefulForm), machine, l => l.IdleTimeout = Idle);

        await bot.Send(1);
        var first = bot.ActiveForm<StatefulForm>(1);
        first.Counter = 7;

        await Evict(bot);

        Assert.Equal(new long[] { 1 }, machine.Saved);
        Assert.Equal(0, bot.Sessions.DormantStateCount);
        Assert.Equal(0, machine.BulkSaves);

        await bot.Send(1);

        Assert.Equal(7, bot.ActiveForm<StatefulForm>(1).Counter);
        Assert.Contains(1L, machine.Loaded);
        Assert.Single(bot.Began);
    }

    [Fact]
    public async Task ManualEnd_DiscardsSavedState()
    {
        var machine = new MemoryDeviceStateMachine();
        var bot = TestBot.Create(typeof(StatefulForm), machine, l => l.IdleTimeout = Idle);

        await bot.Send(1);
        bot.ActiveForm<StatefulForm>(1).Counter = 7;
        await Evict(bot);

        await bot.Send(1);
        await bot.Sessions.EndSessionAsync(1);

        Assert.Contains(1L, machine.Removed);
        Assert.False(machine.Entries.ContainsKey(1));

        await bot.Send(1);
        Assert.Equal(0, bot.ActiveForm<StatefulForm>(1).Counter);
        Assert.Equal(2, bot.Began.Count);
    }

    [Fact]
    public async Task ManualEnd_OfEvictedSession_DiscardsDormantState()
    {
        var machine = new MemoryStateMachine();
        var bot = TestBot.Create(typeof(StatefulForm), machine, l => l.IdleTimeout = Idle);

        await bot.Send(1);
        await Evict(bot);
        Assert.Equal(1, bot.Sessions.DormantStateCount);

        Assert.False(await bot.Sessions.EndSessionAsync(1));

        Assert.Equal(0, bot.Sessions.DormantStateCount);
    }

    [Fact]
    public async Task IgnoreStateForm_IsNotSaved_OnEviction()
    {
        var machine = new MemoryDeviceStateMachine();
        machine.Entries[1] = Entry(1, typeof(TrackingForm), "stale");

        var bot = TestBot.Create(typeof(IgnoredForm), machine, l => l.IdleTimeout = Idle);

        // Restores the stale entry, then navigates to a form which must not be saved.
        await bot.Send(1);
        var session = bot.Sessions.GetSession(1);
        session.ActiveForm = new IgnoredForm { Device = session };

        await Evict(bot);

        Assert.Empty(machine.Saved);
        Assert.False(machine.Entries.ContainsKey(1));
    }

    [Fact]
    public async Task IgnoreStateForm_IsReplacedByFallback_OnEviction()
    {
        var machine = new MemoryStateMachine { FallbackStateForm = typeof(TrackingForm) };
        var bot = TestBot.Create(typeof(IgnoredForm), machine, l => l.IdleTimeout = Idle);

        await bot.Send(1);
        await Evict(bot);

        await bot.Send(1);

        Assert.IsType<TrackingForm>(bot.Sessions.GetSession(1).ActiveForm);
    }

    [Fact]
    public async Task UpdateDuringEviction_WaitsForStateToBeSaved()
    {
        var machine = new MemoryDeviceStateMachine();
        var bot = TestBot.Create(typeof(StatefulForm), machine, l => l.IdleTimeout = Idle);

        await bot.Send(1);
        bot.ActiveForm<StatefulForm>(1).Counter = 99;

        using var saving = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        machine.BeforeSave = _ =>
        {
            saving.Set();
            release.Wait(TimeSpan.FromSeconds(10));
        };

        bot.Advance(TimeSpan.FromDays(1));
        var eviction = Task.Run(() => bot.Sessions.EvictSessions());

        Assert.True(saving.Wait(TimeSpan.FromSeconds(10)));

        // The session is already gone from the list, but its state isn't saved yet.
        Assert.Null(bot.Sessions.GetSession(1));
        var update = Task.Run(() => bot.Send(1));

        await Task.Delay(200);
        Assert.False(update.IsCompleted);

        machine.BeforeSave = null;
        release.Set();

        await eviction;
        await update;

        Assert.Equal(99, bot.ActiveForm<StatefulForm>(1).Counter);
        Assert.Single(bot.Began);
    }

    [Fact]
    public async Task SaveSessionStates_MergesDormantStates_ForPlainStateMachine()
    {
        var machine = new MemoryStateMachine();
        var bot = TestBot.Create(typeof(TrackingForm), machine, l => l.IdleTimeout = Idle);

        await bot.Send(1);
        bot.ActiveForm<TrackingForm>(1).Note = "dormant";
        await Evict(bot);

        await bot.Send(2);
        bot.ActiveForm<TrackingForm>(2).Note = "active";

        await bot.Sessions.SaveSessionStates();

        Assert.Equal(1, machine.BulkSaves);
        var states = machine.Stored.States.ToDictionary(a => a.DeviceId);
        Assert.Equal(new long[] { 1, 2 }, states.Keys.OrderBy(a => a));
        Assert.Equal("dormant", states[1].Values["$Note"]);
        Assert.Equal("active", states[2].Values["$Note"]);
    }

    [Fact]
    public async Task SaveSessionStates_SavesPerDevice_AndKeepsEvictedStates_ForDeviceStateMachine()
    {
        var machine = new MemoryDeviceStateMachine();
        var bot = TestBot.Create(typeof(TrackingForm), machine, l => l.IdleTimeout = Idle);

        await bot.Send(1);
        await Evict(bot);
        await bot.Send(2);

        await bot.Sessions.SaveSessionStates();

        Assert.Equal(0, machine.BulkSaves);
        Assert.Equal(new long[] { 1, 2 }, machine.Entries.Keys.OrderBy(a => a));
    }

    [Fact]
    public async Task Startup_WithoutLimits_RestoresAllSessionsEagerly()
    {
        var machine = new MemoryStateMachine();
        machine.Stored.States.Add(Entry(1, typeof(TrackingForm), "one"));
        machine.Stored.States.Add(Entry(2, typeof(TrackingForm), "two"));

        var bot = TestBot.Create(typeof(TrackingForm), machine);

        await bot.Sessions.LoadSessionStatesOnStart();

        Assert.Equal(2, bot.Sessions.SessionList.Count);
        Assert.Equal("one", bot.ActiveForm<TrackingForm>(1).Note);
        Assert.Equal("chat 2", bot.Sessions.GetSession(2).ChatTitle);
        Assert.Equal(0, bot.Sessions.DormantStateCount);
    }

    [Fact]
    public async Task Startup_WithLimits_KeepsStatesDormantUntilFirstUpdate()
    {
        var machine = new MemoryStateMachine();
        machine.Stored.States.Add(Entry(1, typeof(TrackingForm), "one"));
        machine.Stored.States.Add(Entry(2, typeof(TrackingForm), "two"));

        var bot = TestBot.Create(typeof(TrackingForm), machine, l => l.IdleTimeout = Idle);

        await bot.Sessions.LoadSessionStatesOnStart();

        Assert.Empty(bot.Sessions.SessionList);
        Assert.Empty(bot.Forms.Created);
        Assert.Equal(2, bot.Sessions.DormantStateCount);

        await bot.Send(2);

        Assert.Equal("two", bot.ActiveForm<TrackingForm>(2).Note);
        Assert.Equal(1, bot.Sessions.DormantStateCount);
        Assert.Empty(bot.Began);
    }

    [Fact]
    public async Task Startup_WithLimits_AndDeviceStateMachine_LoadsNothing()
    {
        var machine = new MemoryDeviceStateMachine();
        machine.Entries[1] = Entry(1, typeof(TrackingForm), "one");

        var bot = TestBot.Create(typeof(TrackingForm), machine, l => l.IdleTimeout = Idle);

        await bot.Sessions.LoadSessionStatesOnStart();

        Assert.Equal(0, machine.BulkLoads);
        Assert.Empty(bot.Sessions.SessionList);

        await bot.Sessions.SaveSessionStates();
        Assert.True(machine.Entries.ContainsKey(1));
    }

    [Fact]
    public async Task InvokeMessageLoop_RestoresEvictedSession()
    {
        var machine = new MemoryStateMachine();
        var bot = TestBot.Create(typeof(TrackingForm), machine, l => l.IdleTimeout = Idle);

        await bot.Send(1);
        bot.ActiveForm<TrackingForm>(1).Note = "back";
        await Evict(bot);

        await bot.Bot.InvokeMessageLoop(1);

        Assert.Equal("back", bot.ActiveForm<TrackingForm>(1).Note);
        Assert.Equal(2, bot.Loop.Invocations);
    }

    [Fact]
    public async Task BrokenState_FallsBackToStartForm()
    {
        var machine = new MemoryDeviceStateMachine();
        machine.Entries[1] = new StateEntry
        {
            DeviceId = 1,
            FormUri = "Missing.Form",
            QualifiedName = "Missing.Form, Missing.Assembly"
        };

        var bot = TestBot.Create(typeof(TrackingForm), machine, l => l.IdleTimeout = Idle);

        await bot.Send(1);

        Assert.IsType<TrackingForm>(bot.Sessions.GetSession(1).ActiveForm);
        Assert.Single(bot.Began);
    }
}
