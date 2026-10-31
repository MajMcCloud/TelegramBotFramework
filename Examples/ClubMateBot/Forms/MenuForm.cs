// ClubMateBot demo - SKILL.md 4.2 (AutoCleanForm) + 7.1 (ButtonGrid): the feature catalogue.
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace ClubMateBot.Forms;

/// <summary>
///     The main menu doubles as a live table of contents: every entry maps to one chapter of
///     SKILL.md. Entries whose phase has not been implemented yet answer with the phase number
///     instead of navigating, so the menu is always complete and honest.
/// </summary>
public sealed class MenuForm : AutoCleanForm
{
    private ButtonGrid _menu = null!;

    /// <summary>(label, callback value, phase that delivers it - null means "already implemented").</summary>
    private static readonly (string Label, string Value, string? Phase)[] Catalogue =
    {
        ("Buttons & callbacks", "lab:buttons", null),
        ("Dialogs (modal / non-modal)", "lab:dialogs", null),
        ("Controls gallery", "lab:controls", null),
        ("Events", "events:list", "M3"),
        ("Members", "members:list", "M3"),
        ("Resources & bookings", "resources:list", "M3"),
        ("Event wizard", "events:wizard", "M4"),
        ("Navigation stack", "lab:navigation", "M4"),
        ("State & sessions", "lab:state", "M4"),
        ("Attachments (SentData)", "lab:attachments", "M6"),
        ("Live dashboard", "lab:dashboard", "M6"),
        ("Markdown & charts", "lab:markdown", "M7"),
        ("Error lab", "lab:errors", "M7"),
        ("Settings & language", "settings:open", "M7"),
        ("About this bot", "about", null)
    };

    public MenuForm()
    {
        // AutoCleanForm deletes the bot's previous messages on every call.
        // Caveat: it only tracks what the bot itself sent, never the incoming user message.
        DeleteMode = EDeleteMode.OnEveryCall;
        DeleteSide = EDeleteSide.BotOnly;

        Init += OnInit;
        Opened += OnOpened;
    }

    private Task OnInit(object sender, InitEventArgs e)
    {
        var form = new ButtonForm();

        // Two catalogue buttons per row keeps the keyboard narrow (Telegram allows 8 columns).
        form.AddSplitted(Catalogue.Select(c => new ButtonBase(c.Label, c.Value)), 2);

        _menu = new ButtonGrid(form)
        {
            Title = "ClubMateBot - feature catalogue\n\nPick a demo. Entries with a phase label are " +
                    "stamped with the milestone that implements them.",
            KeyboardType = EKeyboardType.InlineKeyBoard,
            // ButtonGrid confirms the callback query for us, so the button stops spinning.
            ConfirmationText = "Opening..."
        };

        _menu.ButtonClicked += OnButtonClicked;

        AddControl(_menu);

        return Task.CompletedTask;
    }

    private async Task OnOpened(object sender, EventArgs e)
    {
        // Navigation suppresses the message loop's render for this update; display the menu now.
        await _menu.Render(null!);
    }

    private async Task OnButtonClicked(object sender, ButtonClickedEventArgs e)
    {
        var entry = Catalogue.FirstOrDefault(c => c.Value == e.Button?.Value);

        if (entry.Value is null)
        {
            return;
        }

        if (entry.Phase is not null)
        {
            await Device.Send($"'{entry.Label}' is implemented in phase {entry.Phase}.");

            return;
        }

        switch (entry.Value)
        {
            case "lab:buttons":
                await NavigateTo(new ButtonLabForm());
                break;
            case "lab:dialogs":
                await NavigateTo(new DialogLabForm());
                break;
            case "lab:controls":
                await NavigateTo(new ControlsGalleryForm());
                break;
            case "about":
                await Device.Send(BuildAboutText());
                break;
        }
    }

    private string BuildAboutText()
    {
        var stages = string.Join("\n", new[]
        {
            "1. WithAPIKey",
            "2. MiddlewareMessageLoop",
            "3. WithServiceProvider<StartForm>",
            "4. NoProxy",
            "5. UseFullRequestDispatcher",
            "6. CustomCommands",
            "7. UseJSON",
            "8. UseGerman",
            "9. UseSingleThread / UseThreadPool  <- must stay last",
            "10. Build()"
        });

        return $"ClubMateBot - a feature showcase for TelegramBotBase\n\n" +
               $"Builder chain used by this bot:\n{stages}\n\n" +
               $"Sessions: {Program.Bot.Sessions.SessionList.Count}\n" +
               $"Language: {Program.Config.Language}";
    }
}
