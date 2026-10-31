// ClubMateBot demo - SKILL.md 7.1 (CheckedButtonList): multi select for event equipment.
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.DataSources;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace ClubMateBot.Forms;

/// <summary>
///     CheckedButtonList renders a checkbox in front of every row and keeps the checked row indexes
///     inside the control. The sub head row is used as a live counter chip.
///     Note: the check protocol addresses rows by their absolute index in the data source, so this
///     control expects a plain <see cref="ButtonFormDataSource"/>.
/// </summary>
public sealed class EquipmentLabForm : AutoCleanForm
{
    private static readonly string[] Equipment =
    [
        "PA system", "Wireless microphone", "Mixer 16ch", "Monitor speaker", "Moving heads",
        "Uplights", "Fog machine", "LED wall", "Stage 6x4", "Riser", "Dance floor", "Bar counter",
        "Beer cooler", "Espresso machine", "Dishwasher", "Tables 160x80", "Chair sets", "Tents 3x3",
        "Heaters", "Generator 20kVA", "Cables DR", "Power strips", "Extension cords", "Cable ramps",
        "Security fencing", "Signage", "Cloakroom racks", "Umbrellas", "Garden set", "Trash bins"
    ];

    private CheckedButtonList _list = null!;

    public EquipmentLabForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;
        DeleteSide = EDeleteSide.BotOnly;

        Init += OnInit;
        Opened += OnOpened;
    }

    private Task OnInit(object sender, InitEventArgs e)
    {
        var form = new ButtonForm();

        foreach (var item in Equipment)
        {
            form.AddButtonRow(new ButtonBase(item, item.ToLowerInvariant().Replace(' ', '_')));
        }

        _list = new CheckedButtonList
        {
            Title = "Equipment (checked list)\n\nTap a row to toggle the checkbox.",
            DataSource = new ButtonFormDataSource(form),
            KeyboardType = EKeyboardType.InlineKeyBoard,
            ConfirmationText = "Toggled",
            EnablePaging = true,
            NavigationBarVisibility = ENavigationBarVisibility.auto,
            CheckedIconLabel = "✅",
            UncheckedIconLabel = "◻️",
            HeadLayoutButtonRow = new ButtonRow(
                new ButtonBase("Back", "back"),
                new ButtonBase("Show selection", "show"),
                new ButtonBase("Reset", "reset")),
            //The chip is a layout button: its value never changes, only its text.
            SubHeadLayoutButtonRow = new ButtonRow(new ButtonBase("0 checked items", "$"))
        };

        _list.ButtonClicked += OnButtonClicked;
        _list.CheckedChanged += OnCheckedChanged;

        AddControl(_list);

        return Task.CompletedTask;
    }

    private async Task OnOpened(object sender, EventArgs e)
    {
        //Navigation suppresses the message loop's render for this update; display the list now.
        await _list.Render(null!);
    }

    private Task OnCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        //Runs inside the action phase, so the counter chip is part of the render that follows.
        _list.SubHeadLayoutButtonRow = new ButtonRow(
            new ButtonBase($"{_list.CheckedItems.Count} checked items", "$"));

        return Task.CompletedTask;
    }

    private async Task OnButtonClicked(object sender, ButtonClickedEventArgs e)
    {
        var button = e.Button;

        if (button is null)
        {
            return;
        }

        switch (button.Value)
        {
            case "back":
                await NavigateTo(new ControlsGalleryForm());
                return;

            case "show":
                var selected = _list.CheckedItems.Select(a => a.Text).ToList();

                await Device.Send(
                    selected.Count == 0
                        ? "Nothing selected."
                        : $"Selected {selected.Count} item(s):\n- {string.Join("\n- ", selected)}",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.None);
                return;

            case "reset":
                //CheckedButtonList keeps the checked indexes private, so the clean way to start over
                //is a fresh control: navigate to a new instance of this form.
                await Device.Send("Selection reset.");
                await NavigateTo(new EquipmentLabForm());
                return;

            case "$":
                //The counter chip is a layout button - it only reports the current state.
                await Device.Send($"Checked items: {_list.CheckedItems.Count}");
                return;
        }
    }
}
