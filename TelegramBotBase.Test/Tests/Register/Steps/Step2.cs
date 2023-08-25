using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Register.Steps;

public class Step2 : AutoCleanForm
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

        if (UserData.Lastname == null)
        {
            UserData.Lastname = message.MessageText;
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }


    public override async Task Render(MessageResult message)
    {
        if (UserData.Lastname == null)
        {
            await Device.Send("Please sent your lastname:");
            return;
        }

        message.Handled = true;

        var step3 = new Step3();

        step3.UserData = UserData;

        await NavigateTo(step3);
    }
}