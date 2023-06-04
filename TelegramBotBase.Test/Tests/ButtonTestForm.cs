using System;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests;

public class ButtonTestForm : AutoCleanForm
{
    public ButtonTestForm()
    {
        Opened += ButtonTestForm_Opened;
    }

    private async Task ButtonTestForm_Opened(object sender, EventArgs e)
    {
        await Device.Send("Hello world! (Click 'back' to get back to Start)");
    }

    public override async Task Action(MessageResult message)
    {
        var call = message.GetData<CallbackData>();

        await message.ConfirmAction();


        if (call == null)
        {
            return;
        }

        message.Handled = true;

        switch (call.Value)
        {
            case "button1":

                await Device.Send("Button 1 pressed");

                break;

            case "button2":

                await Device.Send("Button 2 pressed");

                break;

            case "button3":

                await Device.Send("Button 3 pressed");

                break;

            case "button4":

                await Device.Send("Button 4 pressed");

                break;

            case "back":

                var st = new Menu();

                await NavigateTo(st);

                break;

            default:

                message.Handled = false;

                break;
        }
    }


    public override async Task Render(MessageResult message)
    {
        var btn = new ButtonForm();

        btn.AddButtonRow(new ButtonBase("Button 1", new CallbackData("a", "button1").Serialize()),
                         new ButtonBase("Button 2", new CallbackData("a", "button2").Serialize()));

        btn.AddButtonRow(new ButtonBase("Button 3", new CallbackData("a", "button3").Serialize()),
                         new ButtonBase("Button 4", new CallbackData("a", "button4").Serialize()));

        btn.AddButtonRow(new ButtonBase("Google.com", "google", "https://www.google.com"),
                         new ButtonBase("Telegram", "telegram", "https://telegram.org/"));

        btn.AddButtonRow(new ButtonBase("Back", new CallbackData("a", "back").Serialize()));

        await Device.Send("Click a button", btn);
    }
}