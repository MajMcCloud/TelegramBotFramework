// ClubMateBot demo - SKILL.md 8 (dialogs): modal/non-modal prompts, confirmations, alerts and array prompts.
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace ClubMateBot.Forms;

/// <summary>Live dialog examples, including which form owns navigation after a dialog opens.</summary>
public sealed class DialogLabForm : AutoCleanForm
{
    private ButtonGrid _grid = null!;

    public DialogLabForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;
        DeleteSide = EDeleteSide.BotOnly;
        Init += OnInit;
        Opened += OnOpened;
    }

    private Task OnInit(object sender, InitEventArgs e)
    {
        var buttons = new ButtonForm();
        buttons.AddButtonRow(new ButtonBase("Modal prompt", "dialog:prompt-modal"));
        buttons.AddButtonRow(new ButtonBase("Non-modal prompt", "dialog:prompt-nonmodal"));
        buttons.AddButtonRow(new ButtonBase("Non-modal confirm", "dialog:confirm"));
        buttons.AddButtonRow(new ButtonBase("Alert dialog", "dialog:alert"));
        buttons.AddButtonRow(new ButtonBase("Array prompt", "dialog:array"));
        buttons.AddButtonRow(new ButtonBase("Show modal navigation error", "dialog:modal-navigation-error"));
        buttons.AddButtonRow(new ButtonBase("Back to menu", "back"));

        _grid = new ButtonGrid(buttons)
        {
            Title = "Dialog lab\n\n" +
                    "Modal dialogs keep their parent form alive. A non-modal dialog replaces it; " +
                    "follow-up navigation must then start from the dialog itself.",
            KeyboardType = EKeyboardType.InlineKeyBoard,
            ConfirmationText = "Opening dialog..."
        };
        _grid.ButtonClicked += OnButtonClicked;
        AddControl(_grid);

        return Task.CompletedTask;
    }

    private async Task OnOpened(object sender, EventArgs e)
    {
        await _grid.Render(null!);
    }

    private async Task OnButtonClicked(object sender, ButtonClickedEventArgs e)
    {
        switch (e.Button?.Value)
        {
            case "dialog:prompt-modal":
                await OpenPrompt(modal: true, demonstrateIllegalNavigation: false);
                break;
            case "dialog:prompt-nonmodal":
                await OpenPrompt(modal: false, demonstrateIllegalNavigation: false);
                break;
            case "dialog:confirm":
                await OpenConfirm();
                break;
            case "dialog:alert":
                await OpenAlert();
                break;
            case "dialog:array":
                await OpenArrayPrompt();
                break;
            case "dialog:modal-navigation-error":
                await OpenPrompt(modal: true, demonstrateIllegalNavigation: true);
                break;
            case "back":
                await NavigateTo(new MenuForm());
                break;
        }
    }

    private async Task OpenPrompt(bool modal, bool demonstrateIllegalNavigation)
    {
        var prompt = new PromptDialog("What name should ClubMateBot use?")
        {
            // CloseForm returns to a parent only for modal prompts; a non-modal prompt has no parent.
            ShowBackButton = modal
        };
        prompt.Opened += async (_, _) => await prompt.Render(null!);

        prompt.Completed += async (sender, args) =>
        {
            if (sender is not PromptDialog dialog)
            {
                return;
            }

            await dialog.Device.Send($"Hello, {args.Value}!");

            if (demonstrateIllegalNavigation)
            {
                try
                {
                    await dialog.NavigateTo(new DialogLabForm());
                    await dialog.Device.Send("Unexpected: modal navigation was allowed.");
                }
                catch (InvalidOperationException)
                {
                    await dialog.Device.Send(
                        "Expected InvalidOperationException: NavigateTo is not allowed from a modal dialog. " +
                        "The parent form remains active.");
                }
            }
            else if (!modal)
            {
                // The original DialogLabForm was closed by NavigateTo(prompt); continue from this dialog.
                await dialog.NavigateTo(new DialogLabForm());
            }
        };

        if (modal)
        {
            await OpenModal(prompt);
        }
        else
        {
            await NavigateTo(prompt);
        }
    }

    private async Task OpenConfirm()
    {
        var dialog = new ConfirmDialog(
            "Should we reserve a place for you?",
            new ButtonBase("Reserve", "reserve"),
            new ButtonBase("Not now", "cancel"))
        {
            AutoCloseOnClick = false
        };
        dialog.Opened += async (_, _) => await dialog.Render(null!);

        dialog.ButtonClicked += async (sender, args) =>
        {
            if (sender is not ConfirmDialog current)
            {
                return;
            }

            var response = args.Button?.Value == "reserve" ? "Place reserved (demo only)." : "No reservation made.";
            await current.Device.Send(response);
            await current.NavigateTo(new DialogLabForm());
        };

        await NavigateTo(dialog);
    }

    private async Task OpenAlert()
    {
        var dialog = new AlertDialog("This is an AlertDialog. It inherits button handling from ConfirmDialog.", "Got it")
        {
            AutoCloseOnClick = false
        };
        dialog.Opened += async (_, _) => await dialog.Render(null!);

        dialog.ButtonClicked += async (sender, _) =>
        {
            if (sender is ConfirmDialog current)
            {
                await current.NavigateTo(new DialogLabForm());
            }
        };

        await NavigateTo(dialog);
    }

    private async Task OpenArrayPrompt()
    {
        var dialog = new ArrayPromptDemo(
            "Choose an event category:",
            new[]
            {
                new ButtonBase("Workshop", "workshop"),
                new ButtonBase("Social", "social")
            },
            new[]
            {
                new ButtonBase("Sports", "sports"),
                    });
                dialog.Opened += async (_, _) => await dialog.Render(null!);

                await NavigateTo(dialog);
    }

    /// <summary>
    ///     ArrayPromptDialog raises a synchronous EventHandler and does not close itself. This adapter
    ///     captures the selected button synchronously, then awaits the response and navigation from
    ///     Action after the base class has completed processing the callback.
    /// </summary>
    private sealed class ArrayPromptDemo : ArrayPromptDialog
    {
        private ButtonBase? _selection;

        public ArrayPromptDemo(string message, params ButtonBase[][] buttons) : base(message, buttons)
        {
            ButtonClicked += (_, args) => _selection = args.Button;
        }

        public override async Task Action(MessageResult message)
        {
            await base.Action(message);

            var selected = _selection;
            _selection = null;

            if (selected is null)
            {
                return;
            }

            var description = selected.Value == "cancel"
                ? "ArrayPromptDialog: cancelled."
                : $"ArrayPromptDialog selection: {selected.Text} ({selected.Value}).";

            await Device.Send(description);
            await NavigateTo(new DialogLabForm());
        }
    }
}
