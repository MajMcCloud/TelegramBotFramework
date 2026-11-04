using TelegramBotBase.Base;
using TelegramBotBase.Commands;
using TelegramBotBase.Builder;
using TelegramBotBase.Localizations;
using TelegramBotBase.MessageLoops;
using TelegramBotBase.RequestDispatchers;
using TelegramBotBase.Sessions;
using TelegramBotBase.Tests.Infrastructure;

namespace TelegramBotBase.Tests;

// Builds touch the global Default.Language, so these tests must not run in parallel with each other.
[Collection(nameof(BuilderTests))]
public class BuilderTests
{
    private const string Token = "1234567890:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    [Fact]
    public void Create_RejectsEmptyApiKey()
    {
        Assert.Throws<ArgumentException>(() => BotBaseBuilder.Create(null));
        Assert.Throws<ArgumentException>(() => BotBaseBuilder.Create(" "));
    }

    [Fact]
    public void Build_WithOnlyStartForm_UsesDefaults()
    {
        var bot = BotBaseBuilder.Create(Token)
                                .WithStartForm<TrackingForm>()
                                .Build();

        Assert.Equal(Token, bot.ApiKey);
        Assert.IsType<MessageClient>(bot.Client);
        Assert.IsType<FormBaseMessageLoop>(bot.MessageLoopFactory);
        Assert.IsType<DefaultRequestDispatcher>(bot.RequestDispatcher);
        Assert.Equal(NetworkingOptions.DefaultTimeout, bot.Client.TelegramClient.Timeout);
        Assert.False(bot.Client.DropPendingUpdates);
        Assert.Equal(bot.MessageLoopFactory.ConfigureUpdateTypes(), bot.Client.AllowedUpdates);
        Assert.Null(bot.StateMachine);
        Assert.Null(bot.SessionLimits);

        // Only /start is registered by default.
        var command = Assert.Single(Assert.Single(bot.BotCommandScopes).Commands);
        Assert.Equal("start", command.Command);
    }

    [Fact]
    public void Build_WithoutStartForm_Throws()
    {
        var ex = Assert.Throws<BotBaseBuilderException>(() => BotBaseBuilder.Create(Token).Build());

        Assert.Contains(ex.Errors, e => e.StartsWith("Start form:"));
    }

    [Fact]
    public void Build_CollectsAllErrors()
    {
        var builder = BotBaseBuilder.Create(Token)
                                    .ConfigureNetworking(n =>
                                    {
                                        n.UseProxy("not a uri");
                                        n.Timeout = TimeSpan.Zero;
                                    })
                                    .UseThreadPool(0, 0)
                                    .WithSessionLimits(l => l.MaxSessions = 0);

        var ex = Assert.Throws<BotBaseBuilderException>(() => builder.Build());

        Assert.Equal(5, ex.Errors.Count);
        Assert.Contains(ex.Errors, e => e.StartsWith("Start form:"));
        Assert.Contains(ex.Errors, e => e.Contains("not a uri"));
        Assert.Contains(ex.Errors, e => e.Contains("timeout"));
        Assert.Contains(ex.Errors, e => e.StartsWith("Threading:"));
        Assert.Contains(ex.Errors, e => e.StartsWith("Session limits:"));
        Assert.All(ex.Errors, e => Assert.Contains(e, ex.Message));
    }

    [Fact]
    public void Build_InvalidStartFormType_Throws()
    {
        var builder = BotBaseBuilder.Create(Token).WithStartForm(typeof(string));

        var ex = Assert.Throws<BotBaseBuilderException>(() => builder.Build());

        Assert.Contains(ex.Errors, e => e.StartsWith("Start form:"));
    }

    [Theory]
    [InlineData("", 8080)]
    [InlineData("127.0.0.1", 0)]
    [InlineData("127.0.0.1", 70000)]
    public void Build_InvalidProxyHostAndPort_Throws(string host, int port)
    {
        var builder = BotBaseBuilder.Create(Token)
                                    .WithStartForm<TrackingForm>()
                                    .ConfigureNetworking(n => n.UseProxy(host, port));

        var ex = Assert.Throws<BotBaseBuilderException>(() => builder.Build());

        Assert.Single(ex.Errors);
    }

    [Fact]
    public void Build_InvalidApiKey_Throws()
    {
        var builder = BotBaseBuilder.Create("no-token").WithStartForm<TrackingForm>();

        var ex = Assert.Throws<BotBaseBuilderException>(() => builder.Build());

        Assert.Contains(ex.Errors, e => e.StartsWith("API key:"));
    }

    [Fact]
    public void Build_Twice_Throws()
    {
        var builder = BotBaseBuilder.Create(Token).WithStartForm<TrackingForm>();
        builder.Build();

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_CustomDispatcherReturningNull_Throws()
    {
        var builder = BotBaseBuilder.Create(Token)
                                    .WithStartForm<TrackingForm>()
                                    .UseCustomRequestDispatcher(_ => null);

        var ex = Assert.Throws<BotBaseBuilderException>(() => builder.Build());

        Assert.Contains(ex.Errors, e => e.StartsWith("Request dispatcher:"));
    }

    [Fact]
    public void Build_MiddlewareLoopReturningNull_Throws()
    {
        var builder = BotBaseBuilder.Create(Token)
                                    .WithStartForm<TrackingForm>()
                                    .MiddlewareMessageLoop(_ => null);

        var ex = Assert.Throws<BotBaseBuilderException>(() => builder.Build());

        Assert.Contains(ex.Errors, e => e.StartsWith("Message loop:"));
    }

    [Fact]
    public void Dispatcher_GetsFinalThreadPoolClient()
    {
        MessageClient dispatcherClient = null;

        var bot = BotBaseBuilder.Create(Token)
                                .WithStartForm<TrackingForm>()
                                .UseCustomRequestDispatcher(c =>
                                {
                                    dispatcherClient = c;
                                    return new DirectDispatcher(c);
                                })
                                .UseThreadPool()
                                .Build();

        Assert.IsType<ThreadPoolMessageClient>(bot.Client);
        Assert.Same(bot.Client, dispatcherClient);
    }

    [Fact]
    public void CallOrder_DoesNotMatter()
    {
        var bot = BotBaseBuilder.Create(Token)
                                .WithSessionLimits(l => l.MaxSessions = 3)
                                .UseThreadPool(4, 2)
                                .ConfigureNetworking(n =>
                                {
                                    n.Timeout = TimeSpan.FromSeconds(5);
                                    n.DropPendingUpdates = true;
                                })
                                .MinimalMessageLoop()
                                .WithStartForm<TrackingForm>()
                                .Build();

        var client = Assert.IsType<ThreadPoolMessageClient>(bot.Client);
        Assert.Equal(4, client.ThreadPool_WorkerThreads);
        Assert.Equal(2, client.ThreadPool_IOThreads);
        Assert.Equal(TimeSpan.FromSeconds(5), client.TelegramClient.Timeout);
        Assert.True(client.DropPendingUpdates);
        Assert.IsType<MinimalMessageLoop>(bot.MessageLoopFactory);
        Assert.Equal(3, bot.SessionLimits.MaxSessions);
    }

    [Fact]
    public void LastCall_Wins()
    {
        var bot = BotBaseBuilder.Create(Token)
                                .WithStartForm<TrackingForm>()
                                .FullMessageLoop()
                                .MinimalMessageLoop()
                                .UseFullRequestDispatcher()
                                .UseDirectDispatcher()
                                .ConfigureNetworking(n => n.UseProxy("not a uri"))
                                .ConfigureNetworking(n => n.UseBotClient(new Telegram.Bot.TelegramBotClient(Token)))
                                .Build();

        Assert.IsType<MinimalMessageLoop>(bot.MessageLoopFactory);
        Assert.IsType<DirectDispatcher>(bot.RequestDispatcher);
    }

    [Fact]
    public void Networking_UsesGivenBotClient()
    {
        var telegramClient = new Telegram.Bot.TelegramBotClient(Token);

        var bot = BotBaseBuilder.Create(Token)
                                .WithStartForm<TrackingForm>()
                                .ConfigureNetworking(n => n.UseBotClient(telegramClient))
                                .Build();

        Assert.Same(telegramClient, bot.Client.TelegramClient);
    }

    [Fact]
    public void AllowedUpdates_ComeFromMessageLoop()
    {
        var bot = BotBaseBuilder.Create(Token)
                                .WithStartForm<TrackingForm>()
                                .MinimalMessageLoop()
                                .UseThreadPool()
                                .Build();

        Assert.Equal(new MinimalMessageLoop().ConfigureUpdateTypes(), bot.Client.AllowedUpdates);
    }

    [Fact]
    public void Commands_CanBeChanged()
    {
        var none = BotBaseBuilder.Create(Token).WithStartForm<TrackingForm>().NoCommands().Build();
        Assert.Empty(none.BotCommandScopes);

        var defaults = BotBaseBuilder.Create(Token).WithStartForm<TrackingForm>().DefaultCommands().Build();
        Assert.Equal(new[] { "start", "help", "settings" },
                     Assert.Single(defaults.BotCommandScopes).Commands.Select(c => c.Command));

        var custom = BotBaseBuilder.Create(Token)
                                   .WithStartForm<TrackingForm>()
                                   .CustomCommands(c => c.Add("myid", "Whats my id?"))
                                   .Build();
        Assert.Equal(new[] { "myid" }, Assert.Single(custom.BotCommandScopes).Commands.Select(c => c.Command));
    }

    [Fact]
    public void Language_IsAppliedOnlyOnBuild()
    {
        var previous = Default.Language;
        try
        {
            Default.Language = new English();

            var builder = BotBaseBuilder.Create(Token).WithStartForm<TrackingForm>().UseGerman();
            Assert.IsType<English>(Default.Language);

            builder.Build();
            Assert.IsType<German>(Default.Language);

            BotBaseBuilder.Create(Token).WithStartForm<TrackingForm>().Build();
            Assert.IsType<German>(Default.Language);
        }
        finally
        {
            Default.Language = previous;
        }
    }

    [Fact]
    public void SessionLimits_AreCopied()
    {
        var limits = new SessionLimits { MaxSessions = 10 };
        var builder = BotBaseBuilder.Create(Token).WithStartForm<TrackingForm>().WithSessionLimits(limits);

        limits.MaxSessions = 1;
        var bot = builder.Build();

        Assert.Equal(10, bot.SessionLimits.MaxSessions);
        Assert.NotSame(limits, bot.SessionLimits);
    }

    [Fact]
    public void Serialization_UsesGivenStateMachine()
    {
        var machine = new States.JsonStateMachine(Path.Combine(Path.GetTempPath(), "tbb-builder-test.json"));

        var bot = BotBaseBuilder.Create(Token).WithStartForm<TrackingForm>().UseSerialization(machine).Build();

        Assert.Same(machine, bot.StateMachine);
    }

    [Fact]
    public void DatabaseExtensions_SetStateMachine()
    {
        var mssql = BotBaseBuilder.Create(Token)
                                  .WithStartForm<TrackingForm>()
                                  .UseSqlServerForTest()
                                  .Build();
        Assert.IsType<Extensions.Serializer.Database.MSSQL.MssqlSerializer>(mssql.StateMachine);

        var postgres = BotBaseBuilder.Create(Token)
                                     .WithStartForm<TrackingForm>()
                                     .UsePostgresForTest()
                                     .Build();
        Assert.IsType<Extensions.Serializer.Database.PostgreSql.PostgreSqlSerializer>(postgres.StateMachine);
    }
}

internal static class BuilderTestExtensions
{
    public static BotBaseBuilder UseSqlServerForTest(this BotBaseBuilder builder)
    {
        return Extensions.Serializer.Database.MSSQL.BotBaseBuilderExtensions.UseSqlDatabase(
            builder, "Server=localhost;Database=tbb;User Id=sa;Password=x;");
    }

    public static BotBaseBuilder UsePostgresForTest(this BotBaseBuilder builder)
    {
        return Extensions.Serializer.Database.PostgreSql.BotBaseBuilderExtensions.UsePostgreSqlDatabase(
            builder, "Host=localhost;Database=tbb;Username=x;Password=x");
    }
}
