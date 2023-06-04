using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Register.Steps;

public class Step3 : AutoCleanForm
{
    public Data UserData { get; set; }

    public override Task Load(MessageResult message)
    {
        if (message.Handled)
        {
            return Task.CompletedTask;
        }

        if (message.MessageText.Trim() == "")
        {
            return Task.CompletedTask;
        }

        if (UserData.EMail == null)
        {
            UserData.EMail = message.MessageText;
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    public override async Task Action(MessageResult message)
    {
        await message.ConfirmAction();

        switch (message.RawData)
        {
            case "back":

                var start = new Start();

                await NavigateTo(start);

                break;
        }
    }

    public override async Task Render(MessageResult message)
    {
        if (UserData.EMail == null)
        {
            await Device.Send("Please sent your email:");
            return;
        }

        message.Handled = true;

        var s = "";

        s += "Firstname: " + UserData.Firstname + "\r\n";
        s += "Lastname: " + UserData.Lastname + "\r\n";
        s += "E-Mail: " + UserData.EMail + "\r\n";

        var bf = new ButtonForm();
        bf.AddButtonRow(new ButtonBase("Back", "back"));

        await Device.Send("Your details:\r\n" + s, bf);
    }
}