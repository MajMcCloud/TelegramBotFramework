using System.Diagnostics;
using Telegram.Bot;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;
using TelegramBotBase.RequestDispatchers;

namespace TelegramBotBase.Example.Tests;

/// <summary>
/// Test form for comparing <see cref="DefaultRequestDispatcher"/> and <see cref="FullRequestDispatcher"/>
/// behavior. For the selected dispatcher, sends, edits, and deletes a user-specified number of messages
/// through that same dispatcher instance and reports elapsed time per phase, so throttling differences
/// are visible in practice
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

    private const string CallbackGroup = "dt";
    private const string ActionDefault = "default";
    private const string ActionFull = "full";
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

        btn.AddButtonRow(new ButtonBase("« Back to menu", new CallbackData(CallbackGroup, ActionBack).Serialize()));

        await Device.Send("Choose a dispatcher test to run:", btn);
    }

    private async Task RequestBurstCount(string dispatcherAction, string label)
    {
        _pendingDispatcherAction = dispatcherAction;

        await Device.Send(
            $"How many messages should the {label} dispatcher burst test use? " +
            $"Enter a number between {MinBurstCount} and {MaxBurstCount}");
        
        _renderMainButtons = false;
    }

    private async Task HandlePendingBurstCountInput(MessageResult message)
    {
        if (_pendingDispatcherAction == null)
        {
            return;
        }

        var text = message.Message?.Text;

        if (!int.TryParse(text, out var count) || count < MinBurstCount || count > MaxBurstCount)
        {
            await Device.Send($"Please enter a valid number between {MinBurstCount} and {MaxBurstCount}");
            _renderMainButtons = false;
            return;
        }

        message.Handled = true;

        var (dispatcher, label) = CreateDispatcher(_pendingDispatcherAction);

        _pendingDispatcherAction = null;

        await RunBurstTest(dispatcher, label, count);
    }

    private (IRequestDispatcher dispatcher, string label) CreateDispatcher(string dispatcherAction)
    {
        return dispatcherAction switch
        {
            ActionFull => (new FullRequestDispatcher(new RequestDispatcherSettings
            {
                MaxRequestsPerSecond = FullDispatcherMaxRequestsPerSecond,
                MaxRequestsPerSecondPerChat = FullDispatcherMaxRequestsPerSecondPerChat,
                MaxRequestsPerMinutePerGroup = FullDispatcherMaxRequestsPerMinutePerGroup
            }, Device.Client), "Full"),

            _ => (new DefaultRequestDispatcher(new RequestDispatcherSettings(), Device.Client), "Default")
        };
    }

    private async Task RunBurstTest(IRequestDispatcher dispatcher, string label, int count)
    {
        var status = await Device.Send($"Running {label} dispatcher burst test ({count} messages)...");

        var totalStopwatch = Stopwatch.StartNew();

        var sendStopwatch = Stopwatch.StartNew();

        var sendTasks = Enumerable.Range(1, count)
            .Select(i => dispatcher.Dispatch(Device,
                a => a.SendMessage(Device.DeviceId, $"{label} burst message #{i}")))
            .ToArray();

        var messages = await Task.WhenAll(sendTasks);

        sendStopwatch.Stop();

        var editStopwatch = Stopwatch.StartNew();

        var editTasks = messages
            .Select((msg, idx) => dispatcher.Dispatch(Device,
                a => a.EditMessageText(Device.DeviceId, msg.MessageId, $"Edited message #{idx + 1}")))
            .ToArray();

        await Task.WhenAll(editTasks);

        editStopwatch.Stop();

        var deleteStopwatch = Stopwatch.StartNew();

        var deleteTasks = messages
            .Select(msg => dispatcher.Dispatch(Device,
                a => a.DeleteMessage(Device.DeviceId, msg.MessageId)))
            .ToArray();

        await Task.WhenAll(deleteTasks);

        deleteStopwatch.Stop();

        totalStopwatch.Stop();

        await Device.Edit(status.MessageId,
            $"{label} dispatcher, {count} messages:\n" +
            $"Send: {sendStopwatch.Elapsed.TotalSeconds:F2}s ({count / sendStopwatch.Elapsed.TotalSeconds:F1} msg/s)\n" +
            $"Edit: {editStopwatch.Elapsed.TotalSeconds:F2}s ({count / editStopwatch.Elapsed.TotalSeconds:F1} msg/s)\n" +
            $"Delete: {deleteStopwatch.Elapsed.TotalSeconds:F2}s ({count / deleteStopwatch.Elapsed.TotalSeconds:F1} msg/s)\n" +
            $"Total: {totalStopwatch.Elapsed.TotalSeconds:F2}s");
    }
}