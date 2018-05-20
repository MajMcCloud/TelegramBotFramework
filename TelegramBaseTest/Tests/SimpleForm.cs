using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBaseTest.Tests
{
    public class SimpleForm : FormBase
    {

        public override async Task Opened()
        {
            await this.Device.Send("Hello world! (send 'back' to get back to Start)");
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
                    await this.Device.Send("Hello you there !");
                    break;

                case "maybe":

                    //Send him a simple message and reply to the one of himself
                    await this.Device.Send("Maybe what?", replyTo: messageId);

                    break;

                case "bye":
                case "ciao":

                    //Send him a simple message
                    await this.Device.Send("Ok, take care !");
                    break;

                case "back":

                    var st = new Start();

                    await st.Init();

                    await this.NavigateTo(st);

                    break;
            }
        }


    }
}
