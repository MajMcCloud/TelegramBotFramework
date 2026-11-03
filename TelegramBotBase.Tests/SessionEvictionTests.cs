using Microsoft.Extensions.DependencyInjection;
using TelegramBotBase.Enums;
using TelegramBotBase.Factories;
using TelegramBotBase.Tests.Infrastructure;

namespace TelegramBotBase.Tests;

public class SessionEvictionTests
{
    private static readonly TimeSpan Idle = TimeSpan.FromMinutes(30);

    [Fact]
    public async Task WithoutLimits_NothingGetsEvicted()
    {
        var bot = TestBot.Create();
        await bot.Send(1);

        bot.Advance(TimeSpan.FromDays(365));

        Assert.Equal(0, await bot.Sessions.EvictSessions());
        Assert.NotNull(bot.Sessions.GetSession(1));
    }

    [Fact]
    public async Task IdleSessions_GetEvictedSilently()
    {
        var bot = TestBot.Create(limits: l => l.IdleTimeout = Idle);
        await bot.Send(1);
        var idleForm = bot.ActiveForm<TrackingForm>(1);

        bot.Advance(TimeSpan.FromMinutes(20));
        await bot.Send(2);

        bot.Advance(TimeSpan.FromMinutes(11));

        Assert.Equal(1, await bot.Sessions.EvictSessions());

        Assert.Null(bot.Sessions.GetSession(1));
        Assert.NotNull(bot.Sessions.GetSession(2));

        Assert.True(idleForm.IsDisposed);
        Assert.Equal(0, idleForm.ClosedCount);

        var ended = Assert.Single(bot.Ended);
        Assert.Equal(1, ended.DeviceId);
        Assert.Equal(ESessionEndReason.Idle, ended.Reason);
    }

    [Fact]
    public async Task SessionExactlyAtTimeout_IsKept()
    {
        var bot = TestBot.Create(limits: l => l.IdleTimeout = Idle);
        await bot.Send(1);

        bot.Advance(Idle);

        Assert.Equal(0, await bot.Sessions.EvictSessions());
    }

    [Fact]
    public async Task EvictedSession_StartsFreshOnNextUpdate_WithoutStateMachine()
    {
        var bot = TestBot.Create(limits: l => l.IdleTimeout = Idle);
        await bot.Send(1);
        var first = bot.ActiveForm<TrackingForm>(1);
        first.Note = "lost";

        bot.Advance(Idle + TimeSpan.FromSeconds(1));
        await bot.Sessions.EvictSessions();

        await bot.Send(1);

        var second = bot.ActiveForm<TrackingForm>(1);
        Assert.NotSame(first, second);
        Assert.Null(second.Note);
        Assert.Equal(2, bot.Began.Count);
    }

    [Fact]
    public async Task SessionInUse_IsNotEvicted()
    {
        var bot = TestBot.Create(limits: l => l.IdleTimeout = Idle);
        await bot.Send(1);
        var form = bot.ActiveForm<TrackingForm>(1);

        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        bot.Loop.Handler = async (_, _, _) =>
        {
            entered.SetResult();
            await release.Task;
        };

        var update = Task.Run(() => bot.Send(1));
        await entered.Task;

        // The update is still processed, while the session looks idle.
        bot.Advance(TimeSpan.FromHours(2));

        Assert.Equal(0, await bot.Sessions.EvictSessions());
        Assert.False(form.IsDisposed);

        release.SetResult();
        await update;

        bot.Advance(TimeSpan.FromHours(2));
        Assert.Equal(1, await bot.Sessions.EvictSessions());
        Assert.True(form.IsDisposed);
    }

    [Fact]
    public async Task KeepAliveForms_AreNotEvicted()
    {
        var bot = TestBot.Create(typeof(KeepAliveForm), limits: l => l.IdleTimeout = Idle);
        await bot.Send(1);

        bot.Advance(TimeSpan.FromDays(1));

        Assert.Equal(0, await bot.Sessions.EvictSessions());
        Assert.NotNull(bot.Sessions.GetSession(1));

        // Can still be ended manually.
        Assert.True(await bot.Sessions.EndSessionAsync(1));
    }

    [Fact]
    public async Task CanEvict_IsRespected()
    {
        var bot = TestBot.Create(typeof(ConditionalForm), limits: l => l.IdleTimeout = Idle);
        await bot.Send(1);
        var form = bot.ActiveForm<ConditionalForm>(1);

        bot.Advance(TimeSpan.FromDays(1));
        Assert.Equal(0, await bot.Sessions.EvictSessions());

        form.AllowEviction = true;
        Assert.Equal(1, await bot.Sessions.EvictSessions());
    }

    [Fact]
    public async Task KeepAlive_OnLinkedForm_KeepsSession()
    {
        var bot = TestBot.Create(typeof(LinkingForm), limits: l => l.IdleTimeout = Idle);
        await bot.Send(1);
        bot.ActiveForm<LinkingForm>(1).Linked = new KeepAliveForm();

        bot.Advance(TimeSpan.FromDays(1));

        Assert.Equal(0, await bot.Sessions.EvictSessions());
    }

    [Fact]
    public async Task MaxSessions_EvictsLeastRecentlyActive_WhenNewSessionStarts()
    {
        var bot = TestBot.Create(limits: l => l.MaxSessions = 3);

        for (long id = 1; id <= 5; id++)
        {
            await bot.Send(id);
            bot.Advance(TimeSpan.FromMinutes(1));
        }

        Assert.Equal(3, bot.Sessions.SessionList.Count);
        Assert.Equal(new long[] { 3, 4, 5 }, bot.Sessions.SessionList.Keys.OrderBy(a => a));
        Assert.Equal(new long[] { 1, 2 }, bot.Ended.Select(a => a.DeviceId));
        Assert.All(bot.Ended, a => Assert.Equal(ESessionEndReason.Capacity, a.Reason));
    }

    [Fact]
    public async Task MaxSessions_UsesRecentActivity_NotStartOrder()
    {
        var bot = TestBot.Create(limits: l => l.MaxSessions = 2);

        await bot.Send(1);
        bot.Advance(TimeSpan.FromMinutes(1));
        await bot.Send(2);
        bot.Advance(TimeSpan.FromMinutes(1));

        // Device 1 becomes the most recently active one.
        await bot.Send(1);
        bot.Advance(TimeSpan.FromMinutes(1));

        await bot.Send(3);

        Assert.Equal(new long[] { 1, 3 }, bot.Sessions.SessionList.Keys.OrderBy(a => a));
        Assert.Equal(2, Assert.Single(bot.Ended).DeviceId);
    }

    [Fact]
    public async Task MaxSessions_CanBeExceeded_WhenSessionsOptOut()
    {
        var bot = TestBot.Create(typeof(KeepAliveForm), limits: l => l.MaxSessions = 1);

        await bot.Send(1);
        await bot.Send(2);

        Assert.Equal(2, bot.Sessions.SessionList.Count);
        Assert.Empty(bot.Ended);
    }

    [Fact]
    public async Task Eviction_DisposesDependencyInjectionScope()
    {
        var services = new ServiceCollection();
        services.AddScoped<ScopedService>();
        var provider = services.BuildServiceProvider();

        var bot = TestBot.Create(formFactory: new ServiceProviderFormFactory(typeof(ScopedForm), provider),
                                 limits: l => l.IdleTimeout = Idle);
        await bot.Send(1);
        var form = bot.ActiveForm<ScopedForm>(1);

        bot.Advance(TimeSpan.FromDays(1));
        await bot.Sessions.EvictSessions();

        Assert.True(form.IsDisposed);
        Assert.True(form.Service.IsDisposed);
        Assert.Equal(0, form.ClosedCount);
    }

    [Fact]
    public async Task Sweeper_EvictsInBackground_AndStops()
    {
        var bot = TestBot.Create(limits: l =>
        {
            l.IdleTimeout = Idle;
            l.SweepInterval = TimeSpan.FromMilliseconds(20);
        });

        await bot.Send(1);
        bot.Advance(TimeSpan.FromDays(1));

        bot.Sessions.StartSweeper();
        try
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (bot.Sessions.GetSession(1) != null && DateTime.UtcNow < deadline)
            {
                await Task.Delay(10);
            }

            Assert.Null(bot.Sessions.GetSession(1));
        }
        finally
        {
            await bot.Sessions.StopSweeper();
        }

        // Stopped: new idle sessions stay.
        await bot.Send(2);
        bot.Advance(TimeSpan.FromDays(1));
        await Task.Delay(100);

        Assert.NotNull(bot.Sessions.GetSession(2));
        Assert.Empty(bot.Errors);
    }

    [Fact]
    public async Task ConcurrentUpdatesAndEvictions_KeepSessionsConsistent()
    {
        var bot = TestBot.Create(limits: l =>
        {
            l.IdleTimeout = TimeSpan.FromMilliseconds(1);
            l.MaxSessions = 5;
        });

        var disposedInUse = 0;
        bot.Loop.Handler = async (_, session, _) =>
        {
            await Task.Yield();

            if (session.ActiveForm.IsDisposed)
            {
                Interlocked.Increment(ref disposedInUse);
            }
        };

        // Real clock: sessions become idle after a millisecond, so the sweeps compete with the updates.
        bot.Sessions.UtcNow = () => DateTime.UtcNow;

        var updates = Enumerable.Range(0, 400).Select(i => Task.Run(() => bot.Send(i % 10)));
        var sweeps = Enumerable.Range(0, 50).Select(_ => Task.Run(() => bot.Sessions.EvictSessions()));

        await Task.WhenAll(updates.Concat(sweeps));

        Assert.Equal(0, disposedInUse);
        Assert.Empty(bot.Errors);
        Assert.True(bot.Sessions.SessionList.Count <= 10);
        Assert.All(bot.Sessions.SessionList.Values, a => Assert.False(a.ActiveForm.IsDisposed));

        // Every created form is either alive in the list or has been disposed.
        var alive = bot.Sessions.SessionList.Values.Select(a => a.ActiveForm).ToHashSet();
        Assert.All(bot.Forms.Created, a => Assert.True(alive.Contains(a) || a.IsDisposed));
    }
}
