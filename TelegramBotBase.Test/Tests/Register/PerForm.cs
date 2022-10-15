using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Register;

public class PerForm : AutoCleanForm
{
    public string EMail { get; set; }

    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public override Task Load(MessageResult message)
    {
        if (message.MessageText.Trim() == "")
        {
            return Task.CompletedTask;
        }

        if (Firstname == null)
        {
            Firstname = message.MessageText;
            return Task.CompletedTask;
        }

        if (Lastname == null)
        {
            Lastname = message.MessageText;
            return Task.CompletedTask;
        }

        if (EMail == null)
        {
            EMail = message.MessageText;
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    public override async Task Action(MessageResult message)
    {
        var call = message.GetData<CallbackData>();

        await message.ConfirmAction();

        if (call == null)
        {
            return;
        }

        switch (call.Value)
        {
            case "back":

                var start = new Start();

                await NavigateTo(start);

                break;
        }
    }

    public override async Task Render(MessageResult message)
    {
        if (Firstname == null)
        {
            await Device.Send("Please sent your firstname:");
            return;
        }

        if (Lastname == null)
        {
            await Device.Send("Please sent your lastname:");
            return;
        }

        if (EMail == null)
        {
            await Device.Send("Please sent your email address:");
            return;
        }


        var s = "";

        s += "Firstname: " + Firstname + "\r\n";
        s += "Lastname: " + Lastname + "\r\n";
        s += "E-Mail: " + EMail + "\r\n";

        var bf = new ButtonForm();
        bf.AddButtonRow(new ButtonBase("Back", new CallbackData("a", "back").Serialize()));

        await Device.Send("Your details:\r\n" + s, bf);
    }
}