using System.Diagnostics;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using TelegramBotBase.Base;
using TelegramBotBase.Constants;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;
using TelegramBotBase.RequestDispatchers;

namespace TelegramBotBase.Example.Tests;

/// <summary>
/// Test form for comparing <see cref="DefaultRequestDispatcher"/> and <see cref="FullRequestDispatcher"/>
/// behavior. For the selected dispatcher, sends, edits, and deletes a user-specified number of messages
/// through that same dispatcher instance and reports elapsed time per phase, so throttling differences
/// are visible in practice. The priority test queues a burst of <see cref="RequestPriority.Background"/>
/// messages and shows that a <see cref="RequestPriority.UI"/> message sent afterwards still goes out first.
/// Every request is isolated: failures, dropped requests (429 retries exhausted) and 429s absorbed by the dispatcher
/// are counted and reported instead of aborting the test
/// </summary>
public class DispatcherTest : AutoCleanForm
{
    // Burst count bounds (actual count is requested from the user) ---
    private const int MinBurstCount = 1;
    private const int MaxBurstCount = 100;

    // "Full" dispatcher rate-limit settings used for the burst comparison ---
    private const int FullDispatcherMaxRequestsPerSecond = 30;
    private const int FullDispatcherMaxRequestsPerSecondPerChat = 1;
    private const int FullDispatcherMaxRequestsPerMinutePerGroup = 20;

    // Upper bound for the per-chat limit the user may pick. Measured against the live API, private chats
    // accept bursts of ~30 messages without a 429, so 1/s is far stricter than Telegram actually is there
    private const int MaxPerChat = 30;

    private const string CallbackGroup = "dt";
    private const string ActionDefault = "default";
    private const string ActionFull = "full";
    private const string ActionPriority = "priority";
    private const string ActionBack = "back";

    // Which dispatcher the next plain-text message should be interpreted as a burst count for
    private string _pendingDispatcherAction;

    private bool _renderMainButtons = true;

    public DispatcherTest()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;
    }

    public override async Task Load(MessageResult message)
    {
        _renderMainButtons = true;
        
        await HandlePendingBurstCountInput(message);
    }

    public override async Task Action(MessageResult message)
    {
        var call = message.GetData<CallbackData>();

        if (call == null)
        {
            await HandlePendingBurstCountInput(message);
            return;
        }

        await message.ConfirmAction();

        message.Handled = true;

        switch (call.Value)
        {
            case ActionDefault:
                await RequestBurstCount(ActionDefault, "Default");
                break;

            case ActionFull:
                await RequestBurstCount(ActionFull, "Full");
                break;

            case ActionPriority:
                await RequestBurstCount(ActionPriority, "priority test (Full dispatcher)");
                break;

            case ActionBack:
                await NavigateTo(new Menu());
                break;

            default:
                message.Handled = false;
                break;
        }
    }

    public override async Task Render(MessageResult message)
    {
        if (!_renderMainButtons) return;
        
        var btn = new ButtonForm();

        btn.AddButtonRow(new ButtonBase("Run burst test - Default dispatcher",
            new CallbackData(CallbackGroup, ActionDefault).Serialize()));

        btn.AddButtonRow(new ButtonBase("Run burst test - Full dispatcher",
            new CallbackData(CallbackGroup, ActionFull).Serialize()));

        btn.AddButtonRow(new ButtonBase("Run priority test - Full dispatcher",
            new CallbackData(CallbackGroup, ActionPriority).Serialize()));

        btn.AddButtonRow(new ButtonBase("« Back to menu", new CallbackData(CallbackGroup, ActionBack).Serialize()));

        await Device.Send("Choose a dispatcher test to run:", btn);
    }

    private async Task RequestBurstCount(string dispatcherAction, string label)
    {
        _pendingDispatcherAction = dispatcherAction;

        await Device.Send(
            $"How many messages should the {label} dispatcher burst test use? " +
            $"Enter a number between {MinBurstCount} and {MaxBurstCount}. " +
            $"Optionally add the per-chat limit for the Full dispatcher, e.g. \"30 5\" " +
            $"(default {FullDispatcherMaxRequestsPerSecondPerChat}/s, max {MaxPerChat}/s)");
        
        _renderMainButtons = false;
    }


    private async Task HandlePendingBurstCountInput(MessageResult message)
    {
        if (_pendingDispatcherAction == null)
        {
            return;
        }

        var parts = (message.Message?.Text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var perChat = FullDispatcherMaxRequestsPerSecondPerChat;

        if (parts.Length is < 1 or > 2
            || !int.TryParse(parts[0], out var count) || count < MinBurstCount || count > MaxBurstCount
            || (parts.Length == 2 && (!int.TryParse(parts[1], out perChat) || perChat < 1 || perChat > MaxPerChat)))
        {
            await Device.Send($"Please enter \"count\" or \"count perChat\": count {MinBurstCount}-{MaxBurstCount}, " +
                              $"perChat 1-{MaxPerChat} (Full dispatcher only)");
            _renderMainButtons = false;
            return;
        }

        message.Handled = true;

        var action = _pendingDispatcherAction;
        var (dispatcher, label) = CreateDispatcher(action, perChat);

        _pendingDispatcherAction = null;

        // A failing test must never break the form: report the error and let the user pick another test
        try
        {
            if (action == ActionPriority)
            {
                await RunPriorityTest(dispatcher, count);
                return;
            }

            await RunBurstTest(dispatcher, label, count);
        }
        catch (Exception ex)
        {
            await TrySend($"{label} test aborted: {Describe(ex)}");
        }
    }

    private (IRequestDispatcher dispatcher, string label) CreateDispatcher(string dispatcherAction, int perChat)
    {
        return dispatcherAction switch
        {
            ActionFull or ActionPriority => (new FullRequestDispatcher(new RequestDispatcherSettings
            {
                MaxRequestsPerSecond = FullDispatcherMaxRequestsPerSecond,
                MaxRequestsPerSecondPerChat = perChat,
                MaxRequestsPerMinutePerGroup = FullDispatcherMaxRequestsPerMinutePerGroup
            }, Device.Client), $"Full ({perChat}/s per chat{(Device.IsGroup ? $", {FullDispatcherMaxRequestsPerMinutePerGroup}/min per group" : "")})"),

            _ => (new DefaultRequestDispatcher(new RequestDispatcherSettings(), Device.Client), "Default")
        };
    }

    private async Task RunBurstTest(IRequestDispatcher dispatcher, string label, int count)
    {
        var status = await Device.Send($"Running {label} dispatcher burst test ({count} messages)...");

        var stats = new PhaseStats();
        var totalStopwatch = Stopwatch.StartNew();
        var report = new StringBuilder($"{label} dispatcher, {count} messages:\n");

        var send = await RunPhase(stats, "Send", Enumerable.Range(1, count)
            .Select(i => (Func<Task<Message>>)(() => dispatcher.Dispatch(Device,
                a => Track(stats, () => a.SendMessage(Device.DeviceId, $"{label} burst message #{i}"))))));
        report.AppendLine(send.Summary);

        // Edit and delete only what was actually sent, so a failed send does not cascade into more failures
        var sent = send.Results;

        try
        {
            var edit = await RunPhase(stats, "Edit", sent
                .Select((msg, idx) => (Func<Task<Message>>)(() => dispatcher.Dispatch(Device,
                    a => Track(stats, () => a.EditMessageText(Device.DeviceId, msg.MessageId, $"Edited message #{idx + 1}"))))));
            report.AppendLine(edit.Summary);
        }
        finally
        {
            var delete = await RunPhase(stats, "Delete", sent
                .Select(msg => (Func<Task<Message>>)(() => dispatcher.Dispatch(Device, a => Track(stats, async () =>
                {
                    await a.DeleteMessage(Device.DeviceId, msg.MessageId);
                    return msg;
                })))));
            report.AppendLine(delete.Summary);
        }

        totalStopwatch.Stop();

        report.AppendLine($"Total: {totalStopwatch.Elapsed.TotalSeconds:F2}s");
        report.Append(stats.ToReport());

        await EditOrSend(status, report.ToString());
    }

    /// <summary>
    /// Queues <paramref name="count"/> background messages, then sends one UI message. With the per-chat limit of
    /// 1 msg/s the background burst takes about <paramref name="count"/> seconds, but the UI message only has to
    /// wait for the slot that is currently cooling down
    /// </summary>
    private async Task RunPriorityTest(IRequestDispatcher dispatcher, int count)
    {
        await Device.Send($"Running priority test: {count} Background messages, then 1 UI message...");

        var stats = new PhaseStats();
        var backgroundStopwatch = Stopwatch.StartNew();

        // Started without awaiting, so the UI message below is queued behind the whole burst
        var backgroundPhase = RunPhase(stats, "Background", Enumerable.Range(1, count)
            .Select(i => (Func<Task<Message>>)(() => dispatcher.Dispatch(Device,
                a => Track(stats, () => a.SendMessage(Device.DeviceId, $"Background message #{i}")),
                priority: RequestPriority.Background))));

        var uiStopwatch = Stopwatch.StartNew();
        var uiPhase = await RunPhase(stats, "UI", new[]
        {
            (Func<Task<Message>>)(() => dispatcher.Dispatch(Device,
                a => Track(stats, () => a.SendMessage(Device.DeviceId, "UI message - sent last, delivered first")),
                priority: RequestPriority.UI))
        });
        uiStopwatch.Stop();

        var background = await backgroundPhase;
        backgroundStopwatch.Stop();

        var cleanup = await RunPhase(stats, "Cleanup", background.Results.Concat(uiPhase.Results)
            .Select(msg => (Func<Task<Message>>)(() => dispatcher.Dispatch(Device, a => Track(stats, async () =>
                {
                    await a.DeleteMessage(Device.DeviceId, msg.MessageId);
                    return msg;
                }),
                priority: RequestPriority.Background))));

        var uiLine = uiPhase.Results.Count == 1
            ? $"UI message delivered after: {uiStopwatch.Elapsed.TotalSeconds:F2}s"
            : "UI message was not delivered";

        await Device.Send(
            $"Priority test, {count} Background messages:\n" +
            $"{uiLine}\n" +
            $"Background burst finished after: {backgroundStopwatch.Elapsed.TotalSeconds:F2}s\n" +
            $"{background.Summary}\n{cleanup.Summary}\n{stats.ToReport()}");
    }

    /// <summary>
    /// Runs every request of a phase concurrently and waits for all of them. Unlike a bare
    /// <see cref="Task.WhenAll(Task[])"/>, one failure neither hides the others nor loses the results that succeeded.
    /// A <c>null</c>/<c>default</c> result means the dispatcher gave up after exhausting its 429 retries
    /// </summary>
    private static async Task<PhaseResult<T>> RunPhase<T>(PhaseStats stats, string name,
        IEnumerable<Func<Task<T>>> requests) where T : class
    {
        var stopwatch = Stopwatch.StartNew();

        var tasks = requests.Select(async request =>
        {
            try
            {
                return (Value: await request(), Error: (Exception)null);
            }
            catch (Exception ex)
            {
                return (Value: (T)null, Error: ex);
            }
        }).ToArray();

        var outcomes = await Task.WhenAll(tasks);
        stopwatch.Stop();

        var result = new PhaseResult<T>(name, stopwatch.Elapsed,
            outcomes.Where(o => o.Value != null).Select(o => o.Value).ToList(),
            outcomes.Count(o => o.Value == null && o.Error == null),
            outcomes.Where(o => o.Error != null).Select(o => o.Error).ToList());

        stats.Record(result.Errors);
        return result;
    }

    /// <summary>Counts 429 responses the dispatcher retried internally; they never reach the caller otherwise</summary>
    private static async Task<T> Track<T>(PhaseStats stats, Func<Task<T>> request)
    {
        try
        {
            return await request();
        }
        catch (ApiRequestException ex) when (ex.ErrorCode == Http.TooManyRequests)
        {
            stats.Record429(ex.Parameters?.RetryAfter);
            throw;
        }
    }

    private async Task EditOrSend(Message status, string text)
    {
        try
        {
            await Device.Edit(status.MessageId, text);
        }
        catch (Exception)
        {
            await TrySend(text);
        }
    }

    private async Task TrySend(string text)
    {
        try
        {
            await Device.Send(text);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DispatcherTest: could not report to chat {Device.DeviceId}: {Describe(ex)}");
        }
    }

    private static string Describe(Exception ex)
    {
        return ex switch
        {
            ApiRequestException api => $"API {api.ErrorCode}: {api.Message}" +
                                       (api.Parameters?.RetryAfter is { } ra ? $" (retry_after {ra}s)" : ""),
            TaskCanceledException => "request timed out or was cancelled",
            _ => $"{ex.GetType().Name}: {ex.Message}"
        };
    }

    private sealed record PhaseResult<T>(string Name, TimeSpan Elapsed, List<T> Results, int Dropped,
        List<Exception> Errors)
    {
        public string Summary
        {
            get
            {
                var seconds = Elapsed.TotalSeconds;
                var line = $"{Name}: {seconds:F2}s, ok {Results.Count} ({Results.Count / Math.Max(seconds, 0.001):F1} msg/s)";

                if (Dropped > 0)
                {
                    line += $", dropped {Dropped} (retries exhausted)";
                }

                if (Errors.Count > 0)
                {
                    line += $", failed {Errors.Count}";
                }

                return line;
            }
        }
    }

    /// <summary>Aggregated diagnostics across all phases of one test run</summary>
    private sealed class PhaseStats
    {
        private readonly object _lock = new();
        private readonly List<int> _retryAfters = new();
        private readonly Dictionary<string, int> _errors = new();
        private int _tooManyRequests;

        public void Record429(int? retryAfter)
        {
            lock (_lock)
            {
                _tooManyRequests++;
                if (retryAfter is { } ra)
                {
                    _retryAfters.Add(ra);
                }
            }
        }

        public void Record(IEnumerable<Exception> errors)
        {
            lock (_lock)
            {
                foreach (var key in errors.Select(DispatcherTest.Describe))
                {
                    _errors[key] = _errors.TryGetValue(key, out var n) ? n + 1 : 1;
                }
            }
        }

        public string ToReport()
        {
            lock (_lock)
            {
                var sb = new StringBuilder();

                sb.Append(_tooManyRequests == 0
                    ? "429 responses: none"
                    : $"429 responses retried by dispatcher: {_tooManyRequests}, retry_after max {(_retryAfters.Count > 0 ? _retryAfters.Max() : 0)}s");

                foreach (var (error, n) in _errors.OrderByDescending(e => e.Value).Take(5))
                {
                    sb.Append($"\n{n}x {error}");
                }

                return sb.ToString();
            }
        }
    }
}
