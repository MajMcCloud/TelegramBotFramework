using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Register.Steps;

public class Step1 : AutoCleanForm
{
    public Step1()
    {
        Init += Step1_Init;
    }

    public Data UserData { get; set; }

    private Task Step1_Init(object sender, InitEventArgs e)
    {
        UserData = new Data();
        return Task.CompletedTask;
    }


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

        if (UserData.Firstname == null)
        {
            UserData.Firstname = message.MessageText;
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    public override async Task Render(MessageResult message)
    {
        if (UserData.Firstname == null)
        {
            await Device.Send("Please sent your firstname:");
            return;
        }

        message.Handled = true;

        var step2 = new Step2();

        step2.UserData = UserData;

        await NavigateTo(step2);
    }
}