using TelegramBotBase.Builder;
using TelegramBotBase.Sessions;
using TelegramBotBase.Tests.Infrastructure;

namespace TelegramBotBase.Tests;

public class SessionLimitsTests
{
    [Fact]
    public void Defaults_HaveNoLimits()
    {
        var bot = TestBot.Create();

        Assert.Null(bot.Bot.SessionLimits);

        var limits = new SessionLimits();
        Assert.False(limits.IsEnabled);
        Assert.Equal(TimeSpan.FromMinutes(1), limits.SweepInterval);
    }

    private const string Token = "1234567890:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    [Fact]
    public void NoSessionLimits_Stage_BuildsWithoutLimits()
    {
        var bot = BotBaseBuilder.Create()
                                .WithAPIKey(Token)
                                .MinimalMessageLoop()
                                .WithStartForm<TrackingForm>()
                                .NoProxy()
                                .UseDirectDispatcher()
                                .NoCommands()
                                .NoSerialization()
                                .DefaultLanguage()
                                .UseSingleThread()
                                .NoSessionLimits()
                                .Build();

        Assert.Null(bot.SessionLimits);
    }

    [Fact]
    public void WithDefaultSessionLimits_Stage_UsesRecommendedDefaults()
    {
        var bot = BotBaseBuilder.Create()
                                .WithAPIKey(Token)
                                .MinimalMessageLoop()
                                .WithStartForm<TrackingForm>()
                                .NoProxy()
                                .UseDirectDispatcher()
                                .NoCommands()
                                .NoSerialization()
                                .DefaultLanguage()
                                .UseSingleThread()
                                .WithDefaultSessionLimits()
                                .Build();

        Assert.Equal(TimeSpan.FromMinutes(30), bot.SessionLimits.IdleTimeout);
        Assert.Equal(10_000, bot.SessionLimits.MaxSessions);
        Assert.Equal(TimeSpan.FromMinutes(1), bot.SessionLimits.SweepInterval);
    }

    [Fact]
    public void CreateDefault_ReturnsIndependentInstances()
    {
        var first = SessionLimits.CreateDefault();
        first.MaxSessions = 1;

        Assert.Equal(SessionLimits.DefaultMaxSessions, SessionLimits.CreateDefault().MaxSessions);
    }

    [Fact]
    public void QuickStart_UsesNoSessionLimits()
    {
        var bot = BotBaseBuilder.Create()
                                .QuickStart<TrackingForm>(Token)
                                .Build();

        Assert.Null(bot.SessionLimits);
    }

    [Fact]
    public void WithSessionLimits_Stage_AfterThreadPool()
    {
        var bot = BotBaseBuilder.Create()
                                .WithAPIKey(Token)
                                .MinimalMessageLoop()
                                .WithStartForm<TrackingForm>()
                                .NoProxy()
                                .UseDirectDispatcher()
                                .NoCommands()
                                .NoSerialization()
                                .DefaultLanguage()
                                .UseThreadPool()
                                .WithSessionLimits(new SessionLimits { MaxSessions = 5 })
                                .Build();

        Assert.Equal(5, bot.SessionLimits.MaxSessions);
        Assert.Null(bot.SessionLimits.IdleTimeout);
    }

    [Fact]
    public void Builder_CopiesLimits()
    {
        var limits = new SessionLimits { IdleTimeout = TimeSpan.FromMinutes(5), MaxSessions = 10 };

        var bot = TestBot.Create(limits: l =>
        {
            l.IdleTimeout = limits.IdleTimeout;
            l.MaxSessions = limits.MaxSessions;
        });

        Assert.True(bot.Bot.SessionLimits.IsEnabled);
        Assert.Equal(TimeSpan.FromMinutes(5), bot.Bot.SessionLimits.IdleTimeout);
        Assert.Equal(10, bot.Bot.SessionLimits.MaxSessions);
    }

    [Theory]
    [InlineData(0, null, 60_000)]
    [InlineData(-1, null, 60_000)]
    [InlineData(null, 0, 60_000)]
    [InlineData(null, -5, 60_000)]
    [InlineData(1_000, null, 0)]
    [InlineData(1_000, null, -1)]
    public void Builder_RejectsInvalidLimits(int? idleMilliseconds, int? maxSessions, int sweepMilliseconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TestBot.Create(limits: l =>
        {
            l.IdleTimeout = idleMilliseconds is { } idle ? TimeSpan.FromMilliseconds(idle) : null;
            l.MaxSessions = maxSessions;
            l.SweepInterval = TimeSpan.FromMilliseconds(sweepMilliseconds);
        }));
    }

    [Fact]
    public void Builder_RejectsTooLongSweepInterval()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TestBot.Create(limits: l =>
        {
            l.IdleTimeout = TimeSpan.FromMinutes(1);
            l.SweepInterval = TimeSpan.FromDays(30);
        }));
    }
}
