using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Builder;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Tests.Infrastructure;

/// <summary>
///     A bot wired with test doubles: no network access, controllable clock, recorded events.
///     Updates are fed directly into the message loop of the bot.
/// </summary>
internal sealed class TestBot
{
    // Syntactically valid token, it is never used for requests.
    private const string Token = "1234567890:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    private readonly object _sync = new();

    private TestBot()
    {
    }

    public BotBase Bot { get; private set; }

    public SessionManager Sessions => Bot.Sessions;

    public TestFormFactory Forms { get; private set; }

    public TestMessageLoop Loop { get; private set; }

    public DateTime Now { get; set; } = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public List<SessionEndEventArgs> Ended { get; } = new();

    public List<long> Began { get; } = new();

    public List<Exception> Errors { get; } = new();

    public static TestBot Create(Type startForm = null, IStateMachine stateMachine = null,
                                 Action<SessionLimits> limits = null, IFormFactory formFactory = null)
    {
        var testBot = new TestBot
        {
            Forms = formFactory as TestFormFactory ?? new TestFormFactory(startForm ?? typeof(TrackingForm)),
            Loop = new TestMessageLoop()
        };

        var serialization = BotBaseBuilder.Create()
                                          .WithAPIKey(Token)
                                          .CustomMessageLoop(testBot.Loop)
                                          .WithFormFactory(formFactory ?? testBot.Forms)
                                          .NoProxy()
                                          .UseDirectDispatcher()
                                          .NoCommands();

        var language = stateMachine == null
                           ? serialization.NoSerialization()
                           : serialization.UseSerialization(stateMachine);

        var limitsStage = language.DefaultLanguage().UseSingleThread();

        var building = limits != null
                           ? limitsStage.WithSessionLimits(limits)
                           : limitsStage.NoSessionLimits();

        testBot.Bot = building.Build();
        testBot.Bot.Sessions.UtcNow = () => testBot.Now;

        testBot.Bot.SessionBegins += (_, e) =>
        {
            lock (testBot._sync)
            {
                testBot.Began.Add(e.DeviceId);
            }
        };

        testBot.Bot.SessionEnds += (_, e) =>
        {
            lock (testBot._sync)
            {
                testBot.Ended.Add(e);
            }
        };

        testBot.Bot.Exception += (_, e) =>
        {
            lock (testBot._sync)
            {
                testBot.Errors.Add(e.Error);
            }
        };

        return testBot;
    }

    /// <summary>
    ///     Processes a text message of the device like an incoming update.
    /// </summary>
    public Task Send(long deviceId, string text = "hello")
    {
        var update = new Update
        {
            Id = 1,
            Message = new Message
            {
                Id = 1,
                Text = text,
                Date = Now,
                Chat = new Chat
                {
                    Id = deviceId,
                    Type = deviceId > 0 ? ChatType.Private : ChatType.Group
                }
            }
        };

        return Bot.Client_MessageLoop(this, new UpdateResult(update, null));
    }

    public void Advance(TimeSpan time)
    {
        Now += time;
    }

    public T ActiveForm<T>(long deviceId) where T : class
    {
        return Sessions.GetSession(deviceId)?.ActiveForm as T;
    }
}
