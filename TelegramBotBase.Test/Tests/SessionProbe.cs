using System.Collections.Concurrent;
using Telegram.Bot;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Builder;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests;

/// <summary>
/// Headless live probe for session limits against the real Telegram Bot API, using a private chat (USER_ID) and a
/// group (CHAT_ID). Telegram doesn't deliver updates from users on demand, so sessions are driven via
/// <see cref="SessionManager.StartSession" /> and <see cref="BotBase.InvokeMessageLoop(long)" />, while the forms send
/// real messages, the bot polls, the sweeper evicts in the background and states go through the JSON state machine.
/// Run with <c>dotnet run -- probe-sessions</c>. All sent messages get deleted at the end.
/// </summary>
public static class SessionProbe
{
    private static readonly TimeSpan IdleTimeout = TimeSpan.FromSeconds(3);

    private static int _failures;

    public static async Task<int> RunAsync(string[] args)
    {
        var apiKey = Require("API_KEY");
        var userId = long.Parse(Require("USER_ID"));
        var chatId = long.Parse(Require("CHAT_ID"));

        var statesFile = Path.Combine(Path.GetTempPath(), $"tgb-session-probe-{Guid.NewGuid():N}.json");

        try
        {
            await ProbeEvictionAndRestore(apiKey, userId, chatId, statesFile);
            await ProbeRestartWithDormantStates(apiKey, chatId, statesFile);
            await ProbeCapacity(apiKey, userId, chatId);
        }
        finally
        {
            await Cleanup(apiKey);

            if (File.Exists(statesFile))
            {
                File.Delete(statesFile);
            }
        }

        Console.WriteLine(_failures == 0 ? "\nAll session checks passed." : $"\n{_failures} session check(s) FAILED.");
        return _failures == 0 ? 0 : 1;
    }

    private static BotBase CreateBot(string apiKey, string statesFile, Action<Sessions.SessionLimits> limits,
                                     List<SessionEndEventArgs> ended, List<Exception> errors)
    {
        var serialization = BotBaseBuilder.Create()
                                          .WithAPIKey(apiKey)
                                          .DefaultMessageLoop()
                                          .WithStartForm<SessionProbeForm>()
                                          .NoProxy()
                                          .UseDefaultRequestDispatcher()
                                          .NoCommands();

        var language = statesFile != null ? serialization.UseJSON(statesFile) : serialization.NoSerialization();

        var bot = language.UseEnglish()
                          .UseSingleThread()
                          .WithSessionLimits(limits)
                          .Build();

        bot.SessionEnds += (_, e) =>
        {
            lock (ended)
            {
                ended.Add(e);
            }

            Console.WriteLine($"  SessionEnds: {e.DeviceId} ({e.Reason})");
        };

        bot.Exception += (_, e) =>
        {
            lock (errors)
            {
                errors.Add(e.Error);
            }

            Console.WriteLine($"  Exception: {e.DeviceId} {e.Error}");
        };

        return bot;
    }

    private static async Task ProbeEvictionAndRestore(string apiKey, long userId, long chatId, string statesFile)
    {
        Console.WriteLine("== Idle eviction, lazy restore and manual end (polling bot, background sweeper)");

        var ended = new List<SessionEndEventArgs>();
        var errors = new List<Exception>();

        var bot = CreateBot(apiKey, statesFile, l =>
        {
            l.IdleTimeout = IdleTimeout;
            l.SweepInterval = TimeSpan.FromMilliseconds(500);
        }, ended, errors);

        await bot.Start();

        try
        {
            await bot.Sessions.StartSession(userId);
            await bot.Sessions.StartSession(chatId);

            foreach (var id in new[] { userId, chatId })
            {
                await bot.InvokeMessageLoop(id);
                await bot.InvokeMessageLoop(id);
            }

            var userForm = (SessionProbeForm)bot.Sessions.GetSession(userId).ActiveForm;
            var chatForm = (SessionProbeForm)bot.Sessions.GetSession(chatId).ActiveForm;

            Check("forms rendered twice and sent real messages",
                  userForm.Renders == 2 && chatForm.Renders == 2 && SessionProbeForm.SentCount == 4,
                  $"renders {userForm.Renders}/{chatForm.Renders}, sent {SessionProbeForm.SentCount}");

            var evicted = await WaitUntil(() => bot.Sessions.SessionList.IsEmpty, TimeSpan.FromSeconds(20));

            Check("background sweeper evicted both idle sessions", evicted,
                  $"{bot.Sessions.SessionList.Count} sessions left");
            Check("evictions reported as Idle",
                  ended.Count == 2 && ended.All(a => a.Reason == ESessionEndReason.Idle),
                  string.Join(", ", ended.Select(a => $"{a.DeviceId}:{a.Reason}")));
            Check("evicted forms disposed silently (no OnClosed)",
                  userForm.IsDisposed && chatForm.IsDisposed && SessionProbeForm.ClosedCalls == 0,
                  $"disposed {userForm.IsDisposed}/{chatForm.IsDisposed}, closed {SessionProbeForm.ClosedCalls}");

            await bot.InvokeMessageLoop(userId);

            var restored = bot.Sessions.GetSession(userId)?.ActiveForm as SessionProbeForm;
            Check("evicted private session restored with its state",
                  restored != null && !ReferenceEquals(restored, userForm) && restored.Renders == 3,
                  $"renders {restored?.Renders}");

            Check("manual end succeeds", await bot.Sessions.EndSessionAsync(userId), "");
            Check("manual end closes the form", SessionProbeForm.ClosedCalls == 1 && restored!.IsDisposed,
                  $"closed {SessionProbeForm.ClosedCalls}");
            Check("manual end reported as Manual", ended.Last().Reason == ESessionEndReason.Manual,
                  ended.Last().Reason.ToString());

            await bot.InvokeMessageLoop(userId);
            Check("manually ended session is not restored", bot.Sessions.GetSession(userId) == null, "");
        }
        finally
        {
            await bot.Stop();
        }

        var json = File.Exists(statesFile) ? await File.ReadAllTextAsync(statesFile) : "";
        Check("stop saved the dormant group state, not the ended private one",
              json.Contains(chatId.ToString()) && !json.Contains(userId.ToString()),
              json.Length == 0 ? "no file" : $"{json.Length} bytes");

        Check("no exceptions", errors.Count == 0, string.Join(" | ", errors.Select(a => a.Message)));
    }

    private static async Task ProbeRestartWithDormantStates(string apiKey, long chatId, string statesFile)
    {
        Console.WriteLine("== Restart: states stay dormant until the device is used again");

        var ended = new List<SessionEndEventArgs>();
        var errors = new List<Exception>();

        var bot = CreateBot(apiKey, statesFile, l => l.IdleTimeout = TimeSpan.FromMinutes(5), ended, errors);

        await bot.Start();

        try
        {
            Check("no session restored eagerly at startup", bot.Sessions.SessionList.IsEmpty,
                  $"{bot.Sessions.SessionList.Count} sessions");

            await bot.InvokeMessageLoop(chatId);

            var form = bot.Sessions.GetSession(chatId)?.ActiveForm as SessionProbeForm;
            Check("group session restored from the JSON file on first use", form is { Renders: 3 },
                  $"renders {form?.Renders}");
        }
        finally
        {
            await bot.Stop();
        }

        Check("no exceptions", errors.Count == 0, string.Join(" | ", errors.Select(a => a.Message)));
    }

    private static async Task ProbeCapacity(string apiKey, long userId, long chatId)
    {
        Console.WriteLine("== Capacity: least recently active session gets evicted");

        var ended = new List<SessionEndEventArgs>();
        var errors = new List<Exception>();

        var bot = CreateBot(apiKey, null, l => l.MaxSessions = 1, ended, errors);

        await bot.Sessions.StartSession(userId);
        await Task.Delay(50);
        await bot.Sessions.StartSession(chatId);

        var evicted = await bot.Sessions.EvictSessions();

        Check("one session evicted", evicted == 1, $"{evicted} evicted");
        Check("the older private session was evicted",
              bot.Sessions.GetSession(userId) == null && bot.Sessions.GetSession(chatId) != null &&
              ended.Single().Reason == ESessionEndReason.Capacity,
              string.Join(", ", ended.Select(a => $"{a.DeviceId}:{a.Reason}")));
        Check("no exceptions", errors.Count == 0, string.Join(" | ", errors.Select(a => a.Message)));
    }

    private static async Task Cleanup(string apiKey)
    {
        var client = new MessageClient(apiKey);
        var deleted = 0;

        foreach (var (chat, messageId) in SessionProbeForm.Sent)
        {
            try
            {
                await client.TelegramClient.DeleteMessage(chat, messageId);
                deleted++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Could not delete message {messageId} in {chat}: {ex.Message}");
            }
        }

        Console.WriteLine($"Cleanup: deleted {deleted}/{SessionProbeForm.Sent.Count} probe messages.");
    }

    private static async Task<bool> WaitUntil(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                return false;
            }

            await Task.Delay(100);
        }

        return true;
    }

    private static void Check(string name, bool ok, string details)
    {
        if (!ok)
        {
            _failures++;
        }

        Console.WriteLine($"  {(ok ? "OK  " : "FAIL")} {name}{(string.IsNullOrEmpty(details) ? "" : $" ({details})")}");
    }

    private static string Require(string name)
    {
        return Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");
    }
}

/// <summary>
/// Sends a message on every render and counts its renders as saved state.
/// </summary>
public class SessionProbeForm : FormBase
{
    private static int _closedCalls;

    public static readonly ConcurrentQueue<(long Chat, int MessageId)> Sent = new();

    public SessionProbeForm()
    {
        Closed += (_, _) =>
        {
            Interlocked.Increment(ref _closedCalls);
            return Task.CompletedTask;
        };
    }

    public static int ClosedCalls => Volatile.Read(ref _closedCalls);

    public static int SentCount => Sent.Count;

    [SaveState]
    public int Renders { get; set; }

    public override async Task Render(MessageResult message)
    {
        Renders++;

        var sent = await Device.Send($"Session probe: render {Renders} in {Device.DeviceId}");
        Sent.Enqueue((Device.DeviceId, sent.MessageId));
    }
}
