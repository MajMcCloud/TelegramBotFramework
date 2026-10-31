// ClubMateBot demo - SKILL.md 7.8 (MultiView): a small booking summary wizard inside one control.
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace ClubMateBot.Forms;

/// <summary>
///     MultiView swaps the content of a control without leaving the form. It tracks every message it sends
///     (through Device.MessageSent) and deletes them when the view changes, so only the current page is visible.
/// </summary>
public sealed class MultiViewLabForm : AutoCleanForm
{
    private BookingSummaryView _view = null!;

    public MultiViewLabForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;
        DeleteSide = EDeleteSide.BotOnly;

        Init += OnInit;
        Opened += OnOpened;
    }

    private Task OnInit(object sender, InitEventArgs e)
    {
        _view = new BookingSummaryView();
        AddControl(_view);

        return Task.CompletedTask;
    }

    private async Task OnOpened(object sender, EventArgs e)
    {
        //Navigation suppresses the message loop's render for this update; show the first page now.
        await _view.Render(null!);
    }

    public override async Task Action(MessageResult message)
    {
        if (_view.Finished)
        {
            //The view instance goes away with the form, so there is nothing to reset.
            await NavigateTo(new ControlsGalleryForm());
        }
    }

    /// <summary>
    ///     Three pages rendered by one control. Callbacks use their own "mv_" prefix so they cannot be
    ///     confused with the protocols of the other hybrid controls.
    /// </summary>
    private sealed class BookingSummaryView : MultiView
    {
        private static readonly string[] Pages =
        {
            "Page 1 of 3 - Event\n\nName: Summer open air\nDate: 2026-07-02\nRoom: Open air",
            "Page 2 of 3 - Organisation\n\nResponsible: Anna\nHelpers: 6 signed up\nEquipment: 2 x PA, 12 tables",
            "Page 3 of 3 - Costs\n\nRental: 800\nCatering: 1200\nTicket revenue: 2600"
        };

        public bool Finished { get; private set; }

        public override async Task Action(MessageResult result, string? value = null)
        {
            switch (result.RawData)
            {
                case "mv_back":
                    //MultiView re-renders automatically once it has rendered at least once.
                    if (SelectedViewIndex > 0)
                    {
                        SelectedViewIndex--;
                    }

                    result.Handled = true;
                    break;
                case "mv_next":
                    if (SelectedViewIndex < Pages.Length - 1)
                    {
                        SelectedViewIndex++;
                    }

                    result.Handled = true;
                    break;
                case "mv_first":
                    SelectedViewIndex = 0;
                    result.Handled = true;
                    break;
                case "mv_done":
                case "mv_cancel":
                    //Leaving the form is up to the form itself, the control only reports the intent.
                    Finished = true;
                    result.Handled = true;
                    break;
            }
        }

        public override async Task RenderView(RenderViewEventArgs e)
        {
            var index = Math.Clamp(e.CurrentView, 0, Pages.Length - 1);

            var buttons = new ButtonForm();
            buttons.AddButtonRow(new ButtonBase("Back", "mv_back"),
                                 new ButtonBase($"Page {index + 1}/{Pages.Length}", "mv_first"),
                                 new ButtonBase("Next", "mv_next"));
            buttons.AddButtonRow(new ButtonBase("Confirm booking", "mv_done"),
                                 new ButtonBase("Cancel", "mv_cancel"));

            await Device.Send(Pages[index], buttons, disableNotification: true,
                              parseMode: ParseMode.None);
        }
    }
}
