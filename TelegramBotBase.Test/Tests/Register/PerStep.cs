using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Example.Tests.Register.Steps;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Register;

public class PerStep : AutoCleanForm
{
    public override async Task Action(MessageResult message)
    {
        await message.ConfirmAction();

        switch (message.RawData)
        {
            case "start":

                var step1 = new Step1();

                await NavigateTo(step1);

                break;
            case "back":

                var start = new Start();

                await NavigateTo(start);

                break;
        }
    }

    public override async Task Render(MessageResult message)
    {
        var bf = new ButtonForm();
        bf.AddButtonRow(new ButtonBase("Goto Step 1", "start"));
        bf.AddButtonRow(new ButtonBase("Back", "back"));

        await Device.Send("Register Steps", bf);
    }
}