using System;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests;

public class SimpleForm : AutoCleanForm
{
    public SimpleForm()
    {
        DeleteSide = EDeleteSide.Both;
        DeleteMode = EDeleteMode.OnLeavingForm;

        Opened += SimpleForm_Opened;
    }

    private async Task SimpleForm_Opened(object sender, EventArgs e)
    {
        await Device.Send("Hello world! (send 'back' to get back to Start)\r\nOr\r\nhi, hello, maybe, bye and ciao");
    }

    public override async Task Load(MessageResult message)
    {
        //message.MessageText will work also, cause it is a string you could manage a lot different scenerios here

        var messageId = message.MessageId;

        switch (message.Command)
        {
            case "hello":
            case "hi":

                //Send him a simple message
                await Device.Send("Hello you there !");
                break;

            case "maybe":

                //Send him a simple message and reply to the one of himself
                await Device.Send("Maybe what?", replyTo: messageId);

                break;

            case "bye":
            case "ciao":

                //Send him a simple message
                await Device.Send("Ok, take care !");
                break;

            case "back":

                var st = new Menu();

                await NavigateTo(st);

                break;
        }
    }
}