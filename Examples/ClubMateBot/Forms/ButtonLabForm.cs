// ClubMateBot demo - SKILL.md 6 (buttons/callbacks) + 7.1 (ButtonGrid): raw and structured callbacks.
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace ClubMateBot.Forms;

/// <summary>
///     Shows inline callbacks and a reply keyboard at the same time. The inline controls confirm
///     callback queries and mark handled updates; reply-keyboard taps arrive as ordinary messages.
/// </summary>
public sealed class ButtonLabForm : AutoCleanForm
{
    private int _counter;
    private ButtonGrid _inlineGrid = null!;
    private ButtonGrid _replyGrid = null!;

    public ButtonLabForm()
    {
        // Keep this form's messages while the user explores; its own messages are cleaned on exit.
        DeleteMode = EDeleteMode.OnLeavingForm;
        DeleteSide = EDeleteSide.BotOnly;
        Init += OnInit;
        Opened += OnOpened;
    }

    private Task OnInit(object sender, InitEventArgs e)
    {
        var inlineButtons = new ButtonForm();
        inlineButtons.AddButtonRow(
            new ButtonBase("Increment (raw)", "raw:increment"),
            new ButtonBase("Show count (JSON)", CallbackData.Create("clubmate", "show-count")));
        inlineButtons.AddButtonRow(
            new ButtonBase("Open dialogs", "open:dialogs"),
            new ButtonBase("Back to menu", "back"));

        _inlineGrid = new ButtonGrid(inlineButtons)
        {
            Title = "Button lab - inline keyboard\n\n" +
                    "Raw string callback: raw:increment\n" +
                    "Structured CallbackData: {m: clubmate, v: show-count}\n" +
                    "ButtonGrid confirms callback queries and marks the update handled.",
            KeyboardType = EKeyboardType.InlineKeyBoard,
            ConfirmationText = "ClubMateBot received the callback"
        };
        _inlineGrid.ButtonClicked += OnInlineButtonClicked;
        AddControl(_inlineGrid);

        var replyButtons = new ButtonForm();
        replyButtons.AddButtonRow("Reply keyboard action", "reply:action");
        replyButtons.AddButtonRow("Hide reply keyboard", "reply:hide");

        _replyGrid = new ButtonGrid(replyButtons)
        {
            Title = "Reply keyboard - taps are regular messages",
            KeyboardType = EKeyboardType.ReplyKeyboard,
            ResizeKeyboard = true,
            OneTimeKeyboard = true,
            DeleteReplyMessage = true
        };
        _replyGrid.ButtonClicked += OnReplyButtonClicked;
        AddControl(_replyGrid);

        return Task.CompletedTask;
    }

    private async Task OnOpened(object sender, EventArgs e)
    {
        // Navigation suppresses the message loop's render for this update; display the controls now.
        await _inlineGrid.Render(null!);
        await _replyGrid.Render(null!);

        var directButtons = new ButtonForm();
        directButtons.AddButtonRow(
            new ButtonBase("Raw form callback", "direct:raw"),
            new ButtonBase("GetData<CallbackData>", CallbackData.Create("direct", "structured")));
        await Device.Send("Form-level Action (no ButtonGrid owns these callbacks):", directButtons);
    }

    public override async Task Action(MessageResult message)
    {
        if (message.Handled || !message.IsAction)
        {
            return;
        }

        if (message.RawData == "direct:raw")
        {
            message.Handled = true;
            await message.ConfirmAction("Raw form callback received");
            await Device.Send($"Form.Action received raw callback: {message.RawData}");
            return;
        }

        var data = message.GetData<CallbackData>();
        if (data?.Method == "direct" && data.Value == "structured")
        {
            message.Handled = true;
            await message.ConfirmAction("Structured form callback received");
            await Device.Send($"Form.Action parsed CallbackData: method='{data.Method}', value='{data.Value}'");
        }
    }

    private async Task OnInlineButtonClicked(object sender, ButtonClickedEventArgs e)
    {
        var value = e.Button?.Value;

        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        if (value == "raw:increment")
        {
            _counter++;
            await Device.Send($"Raw callback received: {value}\nCounter: {_counter}");
            return;
        }

        if (CallbackData.TryDeserialize(value, out var data))
        {
            if (data.Method == "clubmate" && data.Value == "show-count")
            {
                await Device.Send($"Structured callback: method='{data.Method}', value='{data.Value}'.\nCounter: {_counter}");
            }

            return;
        }

        switch (value)
        {
            case "open:dialogs":
                await NavigateTo(new DialogLabForm());
                break;
            case "back":
                await NavigateTo(new MenuForm());
                break;
        }
    }

    private async Task OnReplyButtonClicked(object sender, ButtonClickedEventArgs e)
    {
        switch (e.Button?.Text)
        {
            case "Reply keyboard action":
                _counter++;
                await Device.Send($"Reply keyboard message received. Counter: {_counter}");
                break;
            case "Hide reply keyboard":
                await Device.HideReplyKeyboard();
                await Device.Send("Reply keyboard hidden. Use the inline buttons above to continue.");
                break;
        }
    }
}
