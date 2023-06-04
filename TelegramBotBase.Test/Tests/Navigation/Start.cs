using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Navigation;

public class Start : FormBase
{
    private Message _msg;


    public override Task Load(MessageResult message)
    {
        return Task.CompletedTask;
    }

    public override async Task Action(MessageResult message)
    {
        if (message.Handled)
        {
            return;
        }

        await message.ConfirmAction();

        switch (message.RawData)
        {
            case "yes":

                message.Handled = true;

                //Create navigation controller and navigate to it, keep the current form as root form so we can get back to here later
                var nc = new CustomController(this);
                nc.ForceCleanupOnLastPop = true;

                var f1 = new Form1();

                await nc.PushAsync(f1);

                await NavigateTo(nc);

                if (_msg == null)
                {
                    return;
                }

                await Device.DeleteMessage(_msg);


                break;
            case "no":

                message.Handled = true;

                var mn = new Menu();

                await NavigateTo(mn);

                if (_msg == null)
                {
                    return;
                }

                await Device.DeleteMessage(_msg);

                break;
        }
    }

    public override async Task Render(MessageResult message)
    {
        var bf = new ButtonForm();

        bf.AddButtonRow("Yes", "yes");
        bf.AddButtonRow("No", "no");

        _msg = await Device.Send("Open controller?", bf);
    }
}