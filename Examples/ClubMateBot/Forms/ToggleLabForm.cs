// ClubMateBot demo - SKILL.md 7.5 (ToggleButton, MultiToggleButton) + 7.6 (Label).
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Inline;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace ClubMateBot.Forms;

/// <summary>
///     ToggleButton renders an on/off pair, MultiToggleButton a radio group. Both build their buttons with
///     "new ButtonForm(this)", so their callbacks are routed to the control (#c&lt;id&gt;_on / #c&lt;id&gt;_check$0)
///     and several instances can live on one form without interfering.
/// </summary>
public sealed class ToggleLabForm : AutoCleanForm
{
    private Label _summary = null!;
    private ToggleButton _visible = null!;
    private ToggleButton _notify = null!;
    private MultiToggleButton _size = null!;

    public ToggleLabForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;
        DeleteSide = EDeleteSide.BotOnly;

        Init += OnInit;
        Opened += OnOpened;
    }

    private Task OnInit(object sender, InitEventArgs e)
    {
        _visible = new ToggleButton("Visible", "Hidden")
        {
            Title = "Should this event be listed publicly?",
            Checked = true
        };

        _notify = new ToggleButton("Notify", "Mute")
        {
            Title = "Notify all members about it?",
            Checked = false
        };

        _size = new MultiToggleButton
        {
            Title = "Room size",
            AllowEmptySelection = false,
            Options =
            {
                new ButtonBase("Small room", "small"),
                new ButtonBase("Main hall", "hall"),
                new ButtonBase("Open air", "open")
            },
            SelectedOption = null
        };

        _summary = new Label(BuildSummary(), ParseMode.None);

        //Toggled is a plain (synchronous) EventHandler - keep it cheap and let the next Render cycle do the I/O.
        _visible.Toggled += (_, _) => _summary.Text = BuildSummary();
        _notify.Toggled += (_, _) => _summary.Text = BuildSummary();
        _size.Toggled += (_, _) => _summary.Text = BuildSummary();

        AddControl(_visible);
        AddControl(_notify);
        AddControl(_size);
        AddControl(_summary);

        return Task.CompletedTask;
    }

    private async Task OnOpened(object sender, EventArgs e)
    {
        //Navigation suppresses the message loop's render for this update; display the controls now.
        foreach (var control in Controls)
        {
            await control.Render(null!);
        }
    }

    public override async Task Action(MessageResult message)
    {
        //Callbacks that start with "#c" belong to a control and were already handled there.
        if (message.RawData.StartsWith("#c"))
        {
            return;
        }

        switch (message.RawData)
        {
            case "back":
                message.Handled = true;
                await NavigateTo(new ControlsGalleryForm());
                return;
            case "save":
                message.Handled = true;
                await message.ConfirmAction();
                await Device.Send($"Saved: {BuildSummary()}", parseMode: ParseMode.None);
                _summary.Text = BuildSummary() + "\n\nSaved to this session (not persisted yet - that is phase M3).";
                return;
        }

        if (!message.Handled && message.RawData != "")
        {
            await message.ConfirmAction();
        }
    }

    private string BuildSummary()
    {
        return "Current settings\n" +
               $"Visibility: {(_visible.Checked ? "public" : "internal")}\n" +
               $"Notifications: {(_notify.Checked ? "on" : "off")}\n" +
               $"Room: {_size.SelectedOption?.Text ?? "(not chosen)"}";
    }
}
