// ClubMateBot demo - SKILL.md 7.1 (TreeView / TreeViewNode): event type catalogue.
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Inline;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace ClubMateBot.Forms;

/// <summary>
///     TreeView walks a node hierarchy: nodes with children open the next level, leaf nodes become the
///     selected node. The control keeps its own message; this form adds a status message below it.
/// </summary>
public sealed class TreeLabForm : AutoCleanForm
{
    private int? _statusMessageId;
    private TreeView _tree = null!;

    public TreeLabForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;
        DeleteSide = EDeleteSide.BotOnly;

        Init += OnInit;
        Opened += OnOpened;
    }

    private Task OnInit(object sender, InitEventArgs e)
    {
        _tree = new TreeView
        {
            Title = "Event types\n\nOpen a branch, tap a leaf to select it."
        };

        var privateEvents = new TreeViewNode("Private events", "private",
                                             new TreeViewNode("Birthday", "birthday"),
                                             new TreeViewNode("Wedding", "wedding"),
                                             new TreeViewNode("Anniversary", "anniversary"));

        var corporate = new TreeViewNode("Corporate", "corporate",
                                         new TreeViewNode("Meeting", "meeting"),
                                         new TreeViewNode("Workshop", "workshop",
                                                          new TreeViewNode("Hands on", "hands_on"),
                                                          new TreeViewNode("Seminar room", "seminar_room")),
                                         new TreeViewNode("Team event", "team_event"));

        //A node may carry a URL instead of a callback - Telegram opens the link.
        var publicEvents = new TreeViewNode("Public events", "public",
                                            new TreeViewNode("Concert", "concert"),
                                            new TreeViewNode("Festival", "festival"),
                                            new TreeViewNode("Club website", "url_club", "https://example.com"));

        _tree.Nodes.Add(privateEvents);
        _tree.Nodes.Add(corporate);
        _tree.Nodes.Add(publicEvents);

        AddControl(_tree);

        return Task.CompletedTask;
    }

    private async Task OnOpened(object sender, EventArgs e)
    {
        //Navigation suppresses the message loop's render for this update; display the tree now.
        await _tree.Render(null!);
        await RenderStatusAsync();
    }

    public override async Task Action(MessageResult message)
    {
        if (message.Handled)
        {
            return;
        }

        //The tree confirms and handles its own callbacks; only "back" reaches the form.
        if (message.RawData == "back")
        {
            message.Handled = true;
            await NavigateTo(new ControlsGalleryForm());
        }
    }

    public override async Task Render(MessageResult message)
    {
        await RenderStatusAsync();
    }

    private async Task RenderStatusAsync()
    {
        var text = "Selection state\n" +
                   $"Selected node: {_tree.SelectedNode?.Text ?? "(none)"}\n" +
                   $"Selected value: {_tree.SelectedNode?.Value ?? "(none)"}\n" +
                   $"Visible level: {_tree.VisibleNode?.Text ?? "(root)"}\n" +
                   $"Visible path: {_tree.GetPath()}\n" +
                   $"Selected path: {_tree.SelectedNode?.GetPath() ?? "(none)"}";

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
}
