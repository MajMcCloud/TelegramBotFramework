using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using TelegramBotBase.Base;
using TelegramBotBase.Constants;
using TelegramBotBase.RequestDispatchers;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Example.Tests;

/// <summary>
/// Headless probe that measures the real Telegram Bot API rate limits for a private chat (USER_ID) and a group
/// (CHAT_ID), and checks how the dispatchers behave against them. Run with
/// <c>dotnet run -- probe-limits &lt;scenario&gt; [count] [rate]</c>. Every scenario cleans up the messages it sent
/// </summary>
public static class LimitProbe
{
    private sealed record Attempt(int Index, double StartedAt, double FinishedAt, bool Ok, int? RetryAfter,
        string Error, int MessageId);

    private sealed record ProbeResult(List<Attempt> Attempts)
    {
        public int Ok => Attempts.Count(a => a.Ok);
        public int TooMany => Attempts.Count(a => a.RetryAfter != null);
        public int OtherErrors => Attempts.Count(a => !a.Ok && a.RetryAfter == null);
        public Attempt First429 => Attempts.Where(a => a.RetryAfter != null).OrderBy(a => a.StartedAt).FirstOrDefault();
        public int MaxRetryAfter => Attempts.Max(a => a.RetryAfter ?? 0);
    }

    public static async Task<int> RunAsync(string[] args)
    {
        var apiKey = Require("API_KEY");
        var userId = long.Parse(Require("USER_ID"));
        var chatId = long.Parse(Require("CHAT_ID"));

        var scenario = args.Length > 1 ? args[1] : "help";
        var count = args.Length > 2 ? int.Parse(args[2]) : 30;
        var rate = args.Length > 3 ? double.Parse(args[3], System.Globalization.CultureInfo.InvariantCulture) : 0;

        var client = new MessageClient(apiKey);
        var bot = client.TelegramClient;

        var me = await bot.GetMe();
        Console.WriteLine($"Bot @{me.Username}, private chat {userId}, group {chatId}");

        switch (scenario)
        {
            case "send-private":
                Report($"sendMessage, private, {count} msgs, rate {RateLabel(rate)}",
                    await ProbeSend(bot, userId, count, rate));
                break;

            case "send-group":
                Report($"sendMessage, group, {count} msgs, rate {RateLabel(rate)}",
                    await ProbeSend(bot, chatId, count, rate));
                break;

            case "edit-private":
                Report($"editMessageText, private, {count} edits, rate {RateLabel(rate)}",
                    await ProbeEdit(bot, userId, count, rate));
                break;

            case "edit-group":
                Report($"editMessageText, group, {count} edits, rate {RateLabel(rate)}",
                    await ProbeEdit(bot, chatId, count, rate));
                break;

            case "dispatch-private":
                await ProbeDispatcher(client, userId, false, count, (int)Math.Max(1, rate));
                break;

            case "dispatch-group":
                await ProbeDispatcher(client, chatId, true, count, (int)Math.Max(1, rate));
                break;

            default:
                Console.WriteLine(
                    "Scenarios: send-private | send-group | edit-private | edit-group  [count] [rate msg/s, 0 = all at once]\n" +
                    "           dispatch-private | dispatch-group  [count] [per-chat or per-group limit/s]");
                return 1;
        }

        return 0;
    }

    /// <summary>Starts <paramref name="count"/> sends paced at <paramref name="rate"/>/s and stops starting new ones after the first 429</summary>
    private static async Task<ProbeResult> ProbeSend(ITelegramBotClient bot, long chatId, int count, double rate)
    {
        var result = await Paced(count, rate, async (i, sw) =>
        {
            var m = await bot.SendMessage(chatId, $"probe #{i}");
            return m.MessageId;
        });

        await Cleanup(bot, chatId, result);
        return result;
    }

    /// <summary>Edits a single message <paramref name="count"/> times, paced at <paramref name="rate"/>/s</summary>
    private static async Task<ProbeResult> ProbeEdit(ITelegramBotClient bot, long chatId, int count, double rate)
    {
        var target = await bot.SendMessage(chatId, "edit probe");

        var result = await Paced(count, rate, async (i, sw) =>
        {
            var m = await bot.EditMessageText(chatId, target.MessageId, $"edit probe #{i}");
            return m.MessageId;
        });

        await WaitCooldown(result);
        await Safe(() => bot.DeleteMessage(chatId, target.MessageId));
        return result;
    }

    private static async Task<ProbeResult> Paced(int count, double rate, Func<int, Stopwatch, Task<int>> call)
    {
        var sw = Stopwatch.StartNew();
        var attempts = new List<Attempt>();
        var tasks = new List<Task>();
        var stop = false;

        for (var i = 1; i <= count && !Volatile.Read(ref stop); i++)
        {
            if (rate > 0)
            {
                var due = TimeSpan.FromSeconds((i - 1) / rate) - sw.Elapsed;
                if (due > TimeSpan.Zero)
                {
                    await Task.Delay(due);
                }

                if (Volatile.Read(ref stop))
                {
                    break;
                }
            }

            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                var started = sw.Elapsed.TotalSeconds;
                Attempt attempt;
                try
                {
                    var id = await call(index, sw);
                    attempt = new Attempt(index, started, sw.Elapsed.TotalSeconds, true, null, null, id);
                }
                catch (ApiRequestException ex) when (ex.ErrorCode == Http.TooManyRequests)
                {
                    Volatile.Write(ref stop, true);
                    attempt = new Attempt(index, started, sw.Elapsed.TotalSeconds, false,
                        ex.Parameters?.RetryAfter ?? -1, ex.Message, 0);
                }
                catch (Exception ex)
                {
                    attempt = new Attempt(index, started, sw.Elapsed.TotalSeconds, false, null, ex.Message, 0);
                }

                lock (attempts)
                {
                    attempts.Add(attempt);
                }
            }));
        }

        await Task.WhenAll(tasks);
        attempts.Sort((a, b) => a.Index.CompareTo(b.Index));
        return new ProbeResult(attempts);
    }

    private static void Report(string title, ProbeResult r)
    {
        Console.WriteLine($"\n=== {title} ===");
        Console.WriteLine($"ok={r.Ok} 429={r.TooMany} otherErrors={r.OtherErrors} attempted={r.Attempts.Count}");

        var first = r.First429;
        if (first != null)
        {
            var okBefore = r.Attempts.Count(a => a.Ok && a.StartedAt < first.StartedAt);
            Console.WriteLine(
                $"first 429 at request #{first.Index}, t={first.StartedAt:F2}s, ok before it={okBefore}, retry_after={first.RetryAfter}s (max {r.MaxRetryAfter}s)");
        }

        var lastOk = r.Attempts.Where(a => a.Ok).OrderBy(a => a.FinishedAt).LastOrDefault();
        if (lastOk != null)
        {
            Console.WriteLine($"last ok finished at t={lastOk.FinishedAt:F2}s, effective {r.Ok / Math.Max(lastOk.FinishedAt, 0.001):F1} ok/s");
        }

        var latencies = r.Attempts.Where(a => a.Ok).Select(a => a.FinishedAt - a.StartedAt).OrderBy(x => x).ToArray();
        if (latencies.Length > 0)
        {
            Console.WriteLine(
                $"latency min={latencies[0]:F2}s median={latencies[latencies.Length / 2]:F2}s max={latencies[^1]:F2}s");

            // Completed requests per second of wall time, shows server-side throttling that never returns 429
            var buckets = r.Attempts.Where(a => a.Ok).GroupBy(a => (int)a.FinishedAt).OrderBy(g => g.Key)
                .Select(g => $"{g.Key}s:{g.Count()}");
            Console.WriteLine($"completed per second: {string.Join(" ", buckets)}");
        }

        foreach (var e in r.Attempts.Where(a => !a.Ok && a.RetryAfter == null).Take(3))
        {
            Console.WriteLine($"  error #{e.Index}: {e.Error}");
        }
    }

    /// <summary>
    /// Sends <paramref name="count"/> messages through a <see cref="FullRequestDispatcher"/> configured with
    /// <paramref name="perChat"/> requests per second per chat, counting every 429 the dispatcher hits
    /// internally (those are retried and invisible to the caller otherwise)
    /// </summary>
    private static async Task ProbeDispatcher(MessageClient client, long chatId, bool isGroup, int count, int perChat)
    {
        var settings = new RequestDispatcherSettings
        {
            MaxRequestsPerSecondPerChat = perChat,
            MaxRequestsPerSecondPerGroup = perChat,
            MaxRequestsPerMinutePerGroup = 20
        };

        var dispatcher = new FullRequestDispatcher(settings, client);
        var session = new DeviceSession(chatId, dispatcher);

        var hits429 = 0;
        var retryAfters = new List<int>();
        var failures = new List<string>();
        var dropped = 0;

        var sw = Stopwatch.StartNew();

        var tasks = Enumerable.Range(1, count).Select(async i =>
        {
            try
            {
                var m = await dispatcher.Dispatch(session, async bot =>
                {
                    try
                    {
                        return await bot.SendMessage(chatId, $"dispatch probe #{i}");
                    }
                    catch (ApiRequestException ex) when (ex.ErrorCode == Http.TooManyRequests)
                    {
                        Interlocked.Increment(ref hits429);
                        lock (retryAfters) retryAfters.Add(ex.Parameters?.RetryAfter ?? -1);
                        throw;
                    }
                });

                if (m == null)
                {
                    Interlocked.Increment(ref dropped);
                }

                return m;
            }
            catch (Exception ex)
            {
                lock (failures) failures.Add($"#{i}: {ex.GetType().Name}: {ex.Message}");
                return null;
            }
        }).ToArray();

        var messages = await Task.WhenAll(tasks);
        sw.Stop();

        var sent = messages.Count(m => m != null);

        Console.WriteLine($"\n=== FullRequestDispatcher, {(isGroup ? "group" : "private")}, {count} msgs, perChat={perChat}/s ===");
        Console.WriteLine($"sent={sent} dropped(retries exhausted)={dropped} failed={failures.Count} internal429={hits429} " +
                          $"retry_after=[{string.Join(",", retryAfters)}]");
        Console.WriteLine($"elapsed={sw.Elapsed.TotalSeconds:F2}s, effective {sent / sw.Elapsed.TotalSeconds:F2} msg/s");
        foreach (var f in failures.Take(3)) Console.WriteLine($"  {f}");

        var ids = messages.Where(m => m != null).Select(m => m.MessageId).ToArray();
        await Safe(() => client.TelegramClient.DeleteMessages(chatId, ids));
    }

    private static async Task Cleanup(ITelegramBotClient bot, long chatId, ProbeResult r)
    {
        await WaitCooldown(r);
        var ids = r.Attempts.Where(a => a.Ok).Select(a => a.MessageId).ToArray();
        if (ids.Length > 0)
        {
            await Safe(() => bot.DeleteMessages(chatId, ids));
        }
    }

    private static async Task WaitCooldown(ProbeResult r)
    {
        if (r.MaxRetryAfter > 0)
        {
            Console.WriteLine($"waiting {r.MaxRetryAfter + 1}s for flood control to clear before cleanup...");
            await Task.Delay(TimeSpan.FromSeconds(r.MaxRetryAfter + 1));
        }
    }

    private static async Task Safe(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"cleanup failed: {ex.Message}");
        }
    }

    private static string RateLabel(double rate) => rate > 0 ? $"{rate}/s" : "unpaced";

    private static string Require(string name) =>
        Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");
}
