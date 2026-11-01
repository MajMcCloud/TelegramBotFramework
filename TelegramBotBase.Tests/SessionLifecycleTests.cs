using Microsoft.Extensions.DependencyInjection;
using TelegramBotBase.Enums;
using TelegramBotBase.Factories;
using TelegramBotBase.Form;
using TelegramBotBase.Form.Navigation;
using TelegramBotBase.Tests.Infrastructure;

namespace TelegramBotBase.Tests;

public class SessionLifecycleTests
{
    [Fact]
    public async Task Update_StartsSession_AndTracksLastAction()
    {
        var bot = TestBot.Create();

        await bot.Send(1);

        var session = bot.Sessions.GetSession(1);
        Assert.NotNull(session);
        Assert.Equal(bot.Now, session.LastAction);
        Assert.Equal(new long[] { 1 }, bot.Began);
        Assert.NotNull(session.LastMessage);

        bot.Advance(TimeSpan.FromMinutes(5));
        await bot.Send(1);

        Assert.Same(session, bot.Sessions.GetSession(1));
        Assert.Equal(bot.Now, session.LastAction);
        Assert.Single(bot.Began);
        Assert.Empty(bot.Errors);
    }

    [Fact]
    public async Task GetSession_And_EndSession_ForUnknownDevice_DoNothing()
    {
        var bot = TestBot.Create();

        Assert.Null(bot.Sessions.GetSession(42));
        Assert.False(await bot.Sessions.EndSessionAsync(42));

#pragma warning disable CS0618
        bot.Sessions.EndSession(42);
#pragma warning restore CS0618

        Assert.Empty(bot.Ended);
    }

    [Fact]
    public async Task ConcurrentUpdates_OfNewDevice_StartOneSession()
    {
        var bot = TestBot.Create(typeof(SlowStartForm));

        await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => Task.Run(() => bot.Send(7))));

        Assert.Single(bot.Forms.Created);
        Assert.Single(bot.Began);
        Assert.Single(bot.Sessions.SessionList);
        Assert.Equal(20, bot.Loop.Invocations);
        Assert.Empty(bot.Errors);
    }

    [Fact]
    public async Task EndSessionAsync_ClosesAndDisposesForms_AndRaisesEvent()
    {
        var bot = TestBot.Create();
        await bot.Send(1);
        var form = bot.ActiveForm<TrackingForm>(1);

        Assert.True(await bot.Sessions.EndSessionAsync(1));

        Assert.Null(bot.Sessions.GetSession(1));
        Assert.Equal(1, form.ClosedCount);
        Assert.True(form.IsDisposed);

        var ended = Assert.Single(bot.Ended);
        Assert.Equal(1, ended.DeviceId);
        Assert.Equal(ESessionEndReason.Manual, ended.Reason);

        // The next update starts a fresh session.
        await bot.Send(1);
        Assert.NotSame(form, bot.ActiveForm<TrackingForm>(1));
        Assert.Equal(2, bot.Began.Count);
    }

    [Fact]
    public async Task EndSessionAsync_FromWithinItsOwnUpdate_DefersDisposal()
    {
        var bot = TestBot.Create();
        await bot.Send(1);
        var form = bot.ActiveForm<TrackingForm>(1);

        var disposedDuringUpdate = true;
        var stillUsable = false;

        bot.Loop.Handler = async (b, session, _) =>
        {
            await b.Sessions.EndSessionAsync(session.DeviceId);

            disposedDuringUpdate = form.IsDisposed;
            stillUsable = form.Device != null && form.Client != null;
        };

        await bot.Send(1);

        Assert.False(disposedDuringUpdate);
        Assert.True(stillUsable);
        Assert.True(form.IsDisposed);
        Assert.Equal(1, form.ClosedCount);
        Assert.Null(bot.Sessions.GetSession(1));
        Assert.Single(bot.Ended);
    }

    [Fact]
    public async Task EndSessionAsync_DisposesDependencyInjectionScope()
    {
        var services = new ServiceCollection();
        services.AddScoped<ScopedService>();
        var provider = services.BuildServiceProvider();

        var bot = TestBot.Create(formFactory: new ServiceProviderFormFactory(typeof(ScopedForm), provider));
        await bot.Send(1);
        var form = bot.ActiveForm<ScopedForm>(1);

        Assert.False(form.Service.IsDisposed);

        await bot.Sessions.EndSessionAsync(1);

        Assert.True(form.IsDisposed);
        Assert.True(form.Service.IsDisposed);
    }

    [Fact]
    public async Task EndSessionAsync_DisposesLinkedForms()
    {
        var bot = TestBot.Create();
        await bot.Send(1);
        var session = bot.Sessions.GetSession(1);

        var parent = new LinkingForm { Linked = new TrackingForm() };
        var modal = new TestModalDialog { ParentForm = parent };
        var previous = new TrackingForm();
        var start = session.ActiveForm;

        session.ActiveForm = modal;
        session.PreviousForm = previous;

        await bot.Sessions.EndSessionAsync(1);

        Assert.True(modal.IsDisposed);
        Assert.True(parent.IsDisposed);
        Assert.True(parent.Linked.IsDisposed);
        Assert.True(previous.IsDisposed);

        // Not reachable from the session anymore, it is not ours to dispose.
        Assert.False(start.IsDisposed);
    }

    [Fact]
    public async Task EndSessionAsync_DisposesNavigationStack()
    {
        var bot = TestBot.Create();
        await bot.Send(1);
        var session = bot.Sessions.GetSession(1);

        var root = new TrackingForm();
        var top = new TrackingForm();
        var controller = new NavigationController(root, top);

        session.ActiveForm = controller;

        await bot.Sessions.EndSessionAsync(1);

        Assert.True(controller.IsDisposed);
        Assert.True(root.IsDisposed);
        Assert.True(top.IsDisposed);
    }

    [Fact]
    public async Task FailingSessionEndsHandler_DoesNotPreventDisposal()
    {
        var bot = TestBot.Create();
        await bot.Send(1);
        var form = bot.ActiveForm<TrackingForm>(1);

        bot.Bot.SessionEnds += (_, _) => throw new InvalidOperationException("handler failed");

        Assert.True(await bot.Sessions.EndSessionAsync(1));

        Assert.True(form.IsDisposed);
        Assert.Contains(bot.Errors, a => a is InvalidOperationException);
    }

    [Fact]
    public async Task InvokeMessageLoop_WithoutSession_DoesNothing()
    {
        var bot = TestBot.Create();

        await bot.Bot.InvokeMessageLoop(5);

        Assert.Null(bot.Sessions.GetSession(5));
        Assert.Empty(bot.Forms.Created);
        Assert.Equal(0, bot.Loop.Invocations);
        Assert.Empty(bot.Errors);
    }
}
