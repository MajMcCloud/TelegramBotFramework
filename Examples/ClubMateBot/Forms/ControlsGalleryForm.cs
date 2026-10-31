// ClubMateBot demo - SKILL.md 7 (controls): entry point of the controls gallery.
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Controls.Inline;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace ClubMateBot.Forms;

/// <summary>
///     Hub of the controls gallery. The hub itself already demonstrates three controls:
///     an advanced <see cref="ButtonGrid"/> (paging, navigation bar, head row, keyboard switch),
///     a <see cref="Label"/> as a live status line and the hand written
///     <see cref="ClubMateBot.Controls.QuotaControl"/>.
/// </summary>
public sealed class ControlsGalleryForm : AutoCleanForm
{
    private sealed record Demo(string Label, string Value, string Note);

    private static readonly Demo[] Demos =
    [
        new("Member grid", "lab:members", "ButtonGrid + custom ButtonFormDataSource, paging & search"),
        new("Tagged grid", "lab:tags", "TaggedButtonGrid filtering resources by category"),
        new("Checked list", "lab:checked", "CheckedButtonList for multi select"),
        new("Tree view", "lab:tree", "TreeView / TreeViewNode event type tree"),
        new("Calendar", "lab:calendar", "CalendarPicker and EMonthPickerMode"),
        new("Month picker", "lab:month", "MonthPicker occupancy overview"),
        new("Toggles", "lab:toggle", "ToggleButton and MultiToggleButton"),
        new("Progress bars", "lab:progress", "ProgressBar in every EProgressStyle"),
        new("MultiView", "lab:multiview", "Pages inside one control")
    ];

    private ButtonGrid _grid = null!;
    private Label _status = null!;
    private ClubMateBot.Controls.QuotaControl _quota = null!;

    private int _updatesSeen;

    public ControlsGalleryForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;
        DeleteSide = EDeleteSide.BotOnly;

        Init += OnInit;
        Opened += OnOpened;
    }

    private Task OnInit(object sender, InitEventArgs e)
    {
        //Live status line: Label keeps its own message and edits it whenever Text changes.
        _status = new Label($"Controls gallery - session {Device.DeviceId}, updates seen: 0")
        {
            ParseMode = Telegram.Bot.Types.Enums.ParseMode.None
        };
        AddControl(_status);

        var form = new ButtonForm();

        foreach (var d in Demos)
        {
            form.AddButtonRow(new ButtonBase(d.Label, d.Value));
        }

        _grid = new ButtonGrid(form)
        {
            Title = "Controls gallery\n\n" +
                    "This keyboard is a ButtonGrid with EnablePaging, NavigationBarVisibility=always " +
                    "and a HeadLayoutButtonRow. The row below the header is the navigation bar.",
            KeyboardType = EKeyboardType.InlineKeyBoard,
            ConfirmationText = "Opening...",
            //Layout rows are reserved for the navigation bar and the head row.
            EnablePaging = true,
            NavigationBarVisibility = ENavigationBarVisibility.always,
            HeadLayoutButtonRow = new ButtonRow(
                new ButtonBase("Back to menu", "back"),
                new ButtonBase("Use reply keyboard", "keyboard"))
        };

        _grid.ButtonClicked += OnButtonClicked;

        AddControl(_grid);

        //The custom control from section 7.3.
        _quota = new ClubMateBot.Controls.QuotaControl();
        AddControl(_quota);

        return Task.CompletedTask;
    }

    private async Task OnOpened(object sender, EventArgs e)
    {
        //Navigation suppresses the message loop's render for this update; display the controls now.
        await _status.Render(null!);
        await _grid.Render(null!);
        await _quota.Render(null!);
    }

    public override async Task Action(MessageResult message)
    {
        _updatesSeen++;

        _status.Text = $"Controls gallery - session {Device.DeviceId}, updates seen: {_updatesSeen}";

        if (message.Handled)
        {
            return;
        }

        //The grid confirms the callbacks it knows. The navigation bar's own buttons ($site$ and
        //$search$) are informational, so the form acknowledges them to stop the spinning button.
        switch (message.RawData)
        {
            case "$site$":
                message.Handled = true;
                await message.ConfirmAction($"Page {_grid.CurrentPageIndex + 1} of {_grid.PageCount}");
                break;

            case "$search$":
                message.Handled = true;
                await message.ConfirmAction("Search needs the reply keyboard: switch the keyboard and send a word");
                break;
        }
    }

    private async Task OnButtonClicked(object sender, ButtonClickedEventArgs e)
    {
        var value = e.Button?.Value;

        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        switch (value)
        {
            case "back":
                await NavigateTo(new MenuForm());
                return;

            case "keyboard":
                await SwitchKeyboardTypeAsync();
                return;
        }

        var demo = Demos.FirstOrDefault(d => d.Value == value);

        if (demo is null)
        {
            return;
        }

        await Device.Send($"Opening: {demo.Label}\n{demo.Note}");

        await NavigateTo(demo.Value switch
        {
            "lab:members" => new MemberGridLabForm(),
            "lab:tags" => new TaggedGridLabForm(),
            "lab:checked" => new EquipmentLabForm(),
            "lab:tree" => new TreeLabForm(),
            "lab:calendar" => new CalendarLabForm(),
            "lab:month" => new MonthOccupancyLabForm(),
            "lab:toggle" => new ToggleLabForm(),
            "lab:progress" => new ProgressLabForm(),
            "lab:multiview" => new MultiViewLabForm(),
            _ => new MenuForm()
        });
    }

    private async Task SwitchKeyboardTypeAsync()
    {
        //Changing KeyboardType cleans up the old representation (deletes the inline message or hides
        //the reply keyboard) and marks the control dirty, so it has to be rendered again right away.
        _grid.KeyboardType = _grid.KeyboardType == EKeyboardType.InlineKeyBoard
                                 ? EKeyboardType.ReplyKeyboard
                                 : EKeyboardType.InlineKeyBoard;

        _grid.HeadLayoutButtonRow = new ButtonRow(
            new ButtonBase("Back to menu", "back"),
            new ButtonBase(_grid.KeyboardType == EKeyboardType.InlineKeyBoard
                               ? "Use reply keyboard"
                               : "Use inline keyboard",
                           "keyboard"));

        await _grid.Render(null!);

        await Device.Send(_grid.KeyboardType == EKeyboardType.ReplyKeyboard
                              ? "Reply keyboard active: taps are ordinary messages, paging and search work by text."
                              : "Inline keyboard active: taps are callback queries.");
    }
}
