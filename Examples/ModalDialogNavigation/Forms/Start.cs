using ModalDialogNavigation.Forms.Navigation;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

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
        var bf = new ButtonForm();

        bf.AddButtonRow("V1 (No controls, Modal Prompt Dialog)", "version1");

        bf.AddButtonRow("V2 (ButtonGrid, Modal Prompt Dialog)", "version2");

        bf.AddButtonRow("V3 (ButtonGrid, Non-Modal Prompt Dialog)", "version3");

        bf.AddButtonRow("V4 (ButtonGrid, Modal Prompt Dialog, NavigateTo from Modal)", "version4");

        await Device.Send("Choose your option", bf);
    }
}
