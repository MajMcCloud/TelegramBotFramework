using ModalDialogNavigation.Forms.Navigation;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Markdown;

namespace ModalDialogNavigation.Forms;

public class Start : AutoCleanForm
{
    public override async Task Action(MessageResult message)
    {
        await message.ConfirmAction();

        switch (message.RawData ?? "")
        {
            case "version1":

                var v1 = new Version1();
                await NavigateTo(v1);

                break;

            case "version2":

                var v2 = new Version2();
                await NavigateTo(v2);


                break;

            case "version3":

                var v3 = new Version3();
                await NavigateTo(v3);


                break;

            case "version4":

                var v4 = new Version4();
                await NavigateTo(v4);

                break;
        }
    }

    public override async Task Render(MessageResult message)
    {
        string text_description = $"{"Modal & Dialog Navigation - Test scenarios".Bold()}\r\n\r\n";
        text_description += "This example demonstrates the different ways a PromptDialog can be opened (modal vs. non-modal) and what is (and is not) allowed regarding navigation.\r\n\r\n";
        text_description += "Choose your option:\r\n\r\n";

        text_description += $"{"V1 (No controls, Modal):".Bold()} Renders raw buttons (no ButtonGrid control) and opens the PromptDialog as a modal dialog via OpenModal.\r\n";
        text_description += $"{"Expected:".Bold()} After entering your name you get a greeting. The dialog is modal, so it is deleted together with this form on navigation (a short delay is added to make the greeting visible).\r\n\r\n";

        text_description += $"{"V2 (ButtonGrid, Modal):".Bold()} Uses a ButtonGrid control and opens the PromptDialog as a modal dialog via OpenModal. You can also switch the keyboard type (Inline/Reply).\r\n";
        text_description += $"{"Expected:".Bold()} After entering your name you get a greeting and stay on V2. Control state (e.g. keyboard type) is preserved.\r\n\r\n";

        text_description += $"{"V3 (ButtonGrid, Non-Modal):".Bold()} Uses a ButtonGrid control and opens the PromptDialog via NavigateTo (non-modal). This form is disposed while the dialog is open.\r\n";
        text_description += $"{"Expected:".Bold()} The greeting and follow-up navigation must be done from the PromptDialog context (Completed handler), because V3 no longer exists. It then navigates back to a fresh V3.\r\n\r\n";

        text_description += $"{"V4 (ButtonGrid, Modal + illegal NavigateTo):".Bold()} Uses a ButtonGrid control, opens the PromptDialog modally and then tries to call NavigateTo from within the modal dialog context.\r\n";
        text_description += $"{"Expected:".Bold()} An InvalidOperationException is thrown on purpose - navigating away is not allowed from a modal dialog, only from the parent form. The bot reports whether the exception was raised correctly.\r\n\r\n";


        var bf = new ButtonForm();

        bf.AddButtonRow("V1 (No controls, Modal Prompt Dialog)", "version1");

        bf.AddButtonRow("V2 (ButtonGrid, Modal Prompt Dialog)", "version2");

        bf.AddButtonRow("V3 (ButtonGrid, Non-Modal Prompt Dialog)", "version3");

        bf.AddButtonRow("V4 (ButtonGrid, Modal Prompt Dialog, NavigateTo from Modal)", "version4");

        await Device.Send(text_description, bf);
    }
}
