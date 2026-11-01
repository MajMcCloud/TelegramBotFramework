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
