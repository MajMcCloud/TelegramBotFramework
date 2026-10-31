// ClubMateBot demo - SKILL.md 7.1 (TaggedButtonGrid) + TagButtonBase: resources filtered by category.
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.DataSources;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace ClubMateBot.Forms;

/// <summary>
///     TaggedButtonGrid is a MultiView: view 0 shows the (filtered) items, view 1 the tag picker.
///     The tag picker is opened through the 🏷 button of the navigation bar, so the navigation bar
///     has to be visible - NavigationBarVisibility=always forces it even for a short list.
/// </summary>
public sealed class TaggedGridLabForm : AutoCleanForm
{
    private static readonly (string Name, string Category)[] Resources =
    [
        ("PA system 2kW", "Sound"),
        ("Wireless mic", "Sound"),
        ("Moving heads x4", "Light"),
        ("Uplights x8", "Light"),
        ("Stage 6x4m", "Stage"),
        ("Riser 2x1m", "Stage"),
        ("Beer cooler", "Kitchen"),
        ("Espresso machine", "Kitchen")
    ];

    private TaggedButtonGrid _grid = null!;

    public TaggedGridLabForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;
        DeleteSide = EDeleteSide.BotOnly;

        Init += OnInit;
        Opened += OnOpened;
    }

    private Task OnInit(object sender, InitEventArgs e)
    {
        var form = new ButtonForm();

        for (var i = 0; i < Resources.Length; i++)
        {
            //TagButtonBase carries the category the grid filters by.
            form.AddButtonRow(new TagButtonBase(Resources[i].Name, $"res_{i + 1:00}", Resources[i].Category));
        }

        var tags = Resources.Select(r => r.Category).Distinct().OrderBy(a => a).ToList();

        _grid = new TaggedButtonGrid
        {
            Title = "Resources (tagged grid)\n\nTap 🏷 to open the category filter.",
            DataSource = new ButtonFormDataSource(form),
            KeyboardType = EKeyboardType.InlineKeyBoard,
            ConfirmationText = "Ok",
            EnablePaging = true,
            EnableSearch = true,
            //Without a visible navigation bar there is no 🏷 button to open the tag view.
            NavigationBarVisibility = ENavigationBarVisibility.always,
            EnableCheckAllTools = true,
            Tags = tags,
            SelectedTags = tags.ToList(),
            HeadLayoutButtonRow = new ButtonRow(new ButtonBase("Back", "back"))
        };

        _grid.ButtonClicked += OnButtonClicked;

        AddControl(_grid);

        return Task.CompletedTask;
    }

    private async Task OnOpened(object sender, EventArgs e)
    {
        //Navigation suppresses the message loop's render for this update; display the grid now.
        await _grid.Render(null!);
    }

    private async Task OnButtonClicked(object sender, ButtonClickedEventArgs e)
    {
        var button = e.Button;

        if (button is null)
        {
            return;
        }

        if (button.Value == "back")
        {
            await NavigateTo(new ControlsGalleryForm());
            return;
        }

        if (!button.Value.StartsWith("res_") || !int.TryParse(button.Value["res_".Length..], out var number)
            || number < 1 || number > Resources.Length)
        {
            return;
        }

        var resource = Resources[number - 1];

        await Device.Send(
            $"Row index: {e.Index}\n" +
            $"Resource: {resource.Name}\n" +
            $"Category: {resource.Category}\n" +
            $"Selected categories: {string.Join(", ", _grid.SelectedTags)}\n" +
            $"Visible view: {(_grid.SelectedViewIndex == 0 ? "items" : "tag picker")}",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.None);
    }
}
