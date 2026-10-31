// ClubMateBot demo - SKILL.md 7.4 (MonthPicker): month occupancy overview for the booking calendar.
using System.Globalization;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Inline;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace ClubMateBot.Forms;

/// <summary>
///     MonthPicker is a CalendarPicker locked to the month view (EnableDayView = false), which is what you
///     want for an occupancy overview: picking a month keeps you in the month grid instead of dropping
///     into the day calendar.
/// </summary>
public sealed class MonthOccupancyLabForm : AutoCleanForm
{
    private static readonly (string Name, DateTime Date)[] SampleEvents =
    {
        ("Board meeting", new DateTime(2026, 1, 12)),
        ("Concert night", new DateTime(2026, 1, 24)),
        ("Workshop", new DateTime(2026, 2, 3)),
        ("Wedding", new DateTime(2026, 2, 14)),
        ("Festival prep", new DateTime(2026, 2, 20)),
        ("Festival", new DateTime(2026, 2, 21)),
        ("Maintenance", new DateTime(2026, 4, 5)),
        ("Team event", new DateTime(2026, 6, 18)),
        ("Concert night", new DateTime(2026, 6, 25)),
        ("Summer open air", new DateTime(2026, 7, 2)),
        ("Closed for holidays", new DateTime(2026, 8, 10)),
        ("General assembly", new DateTime(2026, 11, 6)),
        ("Concert night", new DateTime(2026, 11, 21)),
        ("Christmas party", new DateTime(2026, 12, 19))
    };

    private int? _statusMessageId;
    private MonthPicker _picker = null!;

    public MonthOccupancyLabForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;
        DeleteSide = EDeleteSide.BotOnly;

        Init += OnInit;
        Opened += OnOpened;
    }

    private Task OnInit(object sender, InitEventArgs e)
    {
        _picker = new MonthPicker
        {
            Title = "Occupancy by month",
            SelectedDate = new DateTime(2026, 2, 1),
            VisibleMonth = new DateTime(2026, 2, 1),
            Culture = new CultureInfo("de-AT")
        };

        AddControl(_picker);

        return Task.CompletedTask;
    }

    private async Task OnOpened(object sender, EventArgs e)
    {
        //Navigation suppresses the message loop's render for this update; display the picker now.
        await _picker.Render(null!);
        await RenderStatusAsync("Month view stays open when you pick a month.");
    }

    public override async Task Action(MessageResult message)
    {
        if (message.RawData == "back")
        {
            message.Handled = true;
            await NavigateTo(new ControlsGalleryForm());
            return;
        }

        if (!message.Handled && message.RawData != "")
        {
            //The picker does not confirm its own callbacks.
            await message.ConfirmAction();
            await RenderStatusAsync($"Handled '{message.RawData}'.");
        }
    }

    public override async Task Render(MessageResult message)
    {
        await RenderStatusAsync();
    }

    private async Task RenderStatusAsync(string? note = null)
    {
        var year = _picker.VisibleMonth.Year;
        var text = $"Events in {year}\n\n{BuildOccupancy(year, _picker.SelectedDate.Month)}";

        var buttons = new ButtonForm();
        buttons.AddButtonRow(new ButtonBase("Back to gallery", "back"));

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

    private static string BuildOccupancy(int year, int selectedMonth)
    {
        var culture = new CultureInfo("de-AT");
        var lines = new List<string>();

        for (var month = 1; month <= 12; month++)
        {
            var count = SampleEvents.Count(a => a.Date.Year == year && a.Date.Month == month);
            var marker = month == selectedMonth ? ">" : " ";
            lines.Add($"{marker} {culture.DateTimeFormat.MonthNames[month - 1],-12} {new string('#', count)} ({count})");
        }

        var selected = SampleEvents
            .Where(a => a.Date.Year == year && a.Date.Month == selectedMonth)
            .OrderBy(a => a.Date)
            .ToList();

        lines.Add("");
        lines.Add(selected.Count == 0
            ? "No events in the selected month."
            : string.Join("\n", selected.Select(a => $"{a.Date:yyyy-MM-dd}  {a.Name}")));

        return string.Join("\n", lines);
    }
}
