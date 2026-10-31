// ClubMateBot demo - SKILL.md 7.1/7.2: ButtonGrid driven by a custom ButtonFormDataSource,
// EnablePaging, EnableSearch and the reply keyboard options.
using ClubMateBot.Controls;
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace ClubMateBot.Forms;

/// <summary>
///     A member list that is never loaded as a whole. The grid asks the data source for one page of
///     rows at a time, which is the shape a database query needs (phase M3 swaps the in-memory list
///     for EF Core without changing this form).
///     Search is a reply keyboard feature: the grid reads the typed message as the filter.
/// </summary>
public sealed class MemberGridLabForm : AutoCleanForm
{
    private static readonly List<MemberPreview> Members = BuildMembers();

    private ButtonGrid _grid = null!;
    private MemberDataSource _source = null!;

    public MemberGridLabForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;
        DeleteSide = EDeleteSide.BotOnly;

        Init += OnInit;
        Opened += OnOpened;
    }

    private Task OnInit(object sender, InitEventArgs e)
    {
        _source = new MemberDataSource(Members, 2);

        _grid = new ButtonGrid
        {
            Title = "Members (custom data source)\n\n" +
                    "The grid only asks for the rows it shows. Tap 🔍 and type a name or category to " +
                    "filter, use ◀/▶ to page.",
            DataSource = _source,
            //Reply keyboard: paging and search are driven by the message text.
            KeyboardType = EKeyboardType.ReplyKeyboard,
            EnablePaging = true,
            EnableSearch = true,
            NavigationBarVisibility = ENavigationBarVisibility.auto,
            ResizeKeyboard = true,
            OneTimeKeyboard = false,
            DeleteReplyMessage = true,
            HeadLayoutButtonRow = new ButtonRow(
                new ButtonBase("Back", "back"),
                new ButtonBase("Lazy loading stats", "stats"),
                new ButtonBase("Clear filter", "clear"))
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

        switch (button.Value)
        {
            case "back":
                await NavigateTo(new ControlsGalleryForm());
                return;

            case "stats":
                await Device.Send(
                    $"Rows in the source: {_source.RowCount}\n" +
                    $"Members in the source: {_source.TotalMembers}\n" +
                    $"Pages the grid has requested: {_source.PagesRequested}\n" +
                    $"Members turned into buttons: {_source.ItemsMaterialized}\n\n" +
                    "The grid never materialised the whole list - it asked for " +
                    $"{_source.PagesRequested} page(s).",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.None);
                return;

            case "clear":
                _grid.SearchQuery = null;
                _grid.CurrentPageIndex = 0;
                _grid.Updated();
                await _grid.Render(null!);
                await Device.Send("Filter cleared.");
                return;
        }

        //Everything else is a member button: the value is the member id from the data source.
        var member = Members.FirstOrDefault(m => m.Id == button.Value);

        await Device.Send(
            $"Row index from the data source: {e.Index}\n" +
            $"Button text: {button.Text}\n" +
            $"Button value: {button.Value}\n" +
            (member is null
                ? "Member not found in the sample list."
                : $"Member: {member.FirstName} {member.LastName} ({member.Category}), " +
                  $"active: {member.IsActive}"),
            parseMode: Telegram.Bot.Types.Enums.ParseMode.None);
    }

    /// <summary>
    ///     Sample data. Phase M3 replaces this with the EF Core member table.
    /// </summary>
    private static List<MemberPreview> BuildMembers()
    {
        var first = new[]
        {
            "Anna", "Ben", "Clara", "David", "Emma", "Felix", "Greta", "Hannes", "Ida", "Jonas",
            "Lena", "Max", "Nina", "Oskar", "Paula", "Quentin", "Rosa", "Sam", "Timo", "Vera"
        };

        var last = new[]
        {
            "Albers", "Brandt", "Cramer", "Dietrich", "Engel", "Fischer", "Graf", "Hoffmann",
            "Illgen", "Jansen", "Keller", "Lange", "Meier", "Nagel", "Olbert", "Peters",
            "Quade", "Richter", "Scholz", "Thiel", "Uhlig", "Vogt", "Wagner", "Zeidler"
        };

        var categories = new[] { "Active", "Passive", "Youth", "Senior" };

        var members = new List<MemberPreview>();

        for (var i = 0; i < 48; i++)
        {
            members.Add(new MemberPreview(
                            $"m{i + 1:00}",
                            first[i % first.Length],
                            last[i % last.Length],
                            categories[i % categories.Length],
                            i % 7 != 0));
        }

        return members;
    }
}
