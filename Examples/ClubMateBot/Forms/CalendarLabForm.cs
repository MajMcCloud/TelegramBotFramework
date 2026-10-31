// ClubMateBot demo - SKILL.md 7.4 (CalendarPicker): booking date picker with all three views.
using System.Globalization;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Inline;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace ClubMateBot.Forms;

/// <summary>
///     CalendarPicker walks day -> month -> year. The control never confirms or marks the callback as
///     handled, so the form acknowledges it and reports the new selection.
/// </summary>
public sealed class CalendarLabForm : AutoCleanForm
{
    private int? _statusMessageId;
    private CalendarPicker _picker = null!;

    public CalendarLabForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;
        DeleteSide = EDeleteSide.BotOnly;

        Init += OnInit;
        Opened += OnOpened;
    }

    private Task OnInit(object sender, InitEventArgs e)
    {
        _picker = new CalendarPicker
        {
            Title = "Pick the booking date",
            SelectedDate = DateTime.Today,
            VisibleMonth = DateTime.Today,
            FirstDayOfWeek = DayOfWeek.Monday,
            //Month and weekday names come from this culture - the framework is not hard coded to English.
            Culture = new CultureInfo("de-AT"),
            EnableDayView = true,
            EnableMonthView = true,
            EnableYearView = true
        };

        AddControl(_picker);

        return Task.CompletedTask;
    }

    private async Task OnOpened(object sender, EventArgs e)
    {
        //Navigation suppresses the message loop's render for this update; display the picker now.
        await _picker.Render(null!);
        await RenderStatusAsync("Picker opened.");
    }

    public override async Task Action(MessageResult message)
    {
        switch (message.RawData)
        {
            case "back":
                message.Handled = true;
                await NavigateTo(new ControlsGalleryForm());
                return;
            case "mode_day":
                message.Handled = true;
                _picker.PickerMode = EMonthPickerMode.day;
                await RenderStatusAsync("Switched to the day view programmatically.");
                return;
            case "mode_month":
                message.Handled = true;
                _picker.PickerMode = EMonthPickerMode.month;
                await RenderStatusAsync("Switched to the month view programmatically.");
                return;
            case "mode_year":
                message.Handled = true;
                _picker.PickerMode = EMonthPickerMode.year;
                await RenderStatusAsync("Switched to the year view programmatically.");
                return;
            case "reset":
                message.Handled = true;
                _picker.SelectedDate = DateTime.Today;
                _picker.VisibleMonth = DateTime.Today;
                await RenderStatusAsync("Selection reset to today.");
                return;
        }

        //Anything else came from the picker itself. It neither confirms nor marks it handled, so the
        //button would keep spinning - acknowledge it here and surface the new state.
        if (!message.Handled && message.RawData != "")
        {
            await message.ConfirmAction();
            await RenderStatusAsync($"Picker handled '{message.RawData}'.");
        }
    }

    public override async Task Render(MessageResult message)
    {
        await RenderStatusAsync();
    }

    private async Task RenderStatusAsync(string? note = null)
    {
        var text = "Booking date\n" +
                   $"Selected: {_picker.SelectedDate:yyyy-MM-dd}\n" +
                   $"Visible month: {_picker.VisibleMonth:yyyy-MM}\n" +
                   $"Current view: {_picker.PickerMode}\n" +
                   $"Culture: {_picker.Culture.Name}, first day: {_picker.FirstDayOfWeek}" +
                   (note == null ? "" : $"\n\n{note}");

        var buttons = new ButtonForm();
        buttons.AddButtonRow(new ButtonBase("Day view", "mode_day"),
                             new ButtonBase("Month view", "mode_month"),
                             new ButtonBase("Year view", "mode_year"));
        buttons.AddButtonRow(new ButtonBase("Today", "reset"),
                             new ButtonBase("Back to gallery", "back"));

        if (_statusMessageId != null)
        {
            var edited = await Device.Edit(_statusMessageId.Value, text, buttons,
                                           parseMode: Telegram.Bot.Types.Enums.ParseMode.None);

            if (edited != null)
            {
                return;
            }
        }

        var sent = await Device.Send(text, buttons, disableNotification: true,
                                     parseMode: Telegram.Bot.Types.Enums.ParseMode.None);

        if (sent != null)
        {
            _statusMessageId = sent.MessageId;
        }
    }
}
