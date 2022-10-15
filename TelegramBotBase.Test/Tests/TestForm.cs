using System;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests;

public class TestForm : FormBase
{
    public TestForm()
    {
        Opened += TestForm_Opened;
        Closed += TestForm_Closed;
    }

    private string LastMessage { get; set; }

    private async Task TestForm_Opened(object sender, EventArgs e)
    {
        await Device.Send("Welcome to Form 1");
    }

    private async Task TestForm_Closed(object sender, EventArgs e)
    {
        await Device.Send("Ciao from Form 1");
    }


    public override async Task Action(MessageResult message)
    {
        switch (message.Command)
        {
            case "reply":


                break;

            case "navigate":

                var tf = new TestForm2();

                await NavigateTo(tf);

                break;

            default:

                if (message.UpdateData == null)
                {
                    return;
                }

                LastMessage = message.Message.Text;

                break;
        }
    }


    public override async Task Render(MessageResult message)
    {
        if (message.Command == "reply")
        {
            await Device.Send("Last message: " + LastMessage);
        }
    }
}