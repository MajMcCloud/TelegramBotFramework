using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Tests
{
    public class TestForm : FormBase
    {


        String LastMessage { get; set; }

        public override async Task Init(params object[] param)
        {
            
        }

        public override async Task Opened()
        {
            await this.Device.Send("Willkommen in Formular 1");
        }

        public override async Task Closed()
        {
            await this.Device.Send("Tschüss in Formular 1");
        }

        public override async Task Load(MessageResult message)
        {


        }


        public override async Task Action(MessageResult message)
        {
            switch (message.Command)
            {
                case "reply":

                    

                    break;

                case "navigate":

                    var tf = new TestForm2();

                    await tf.Init();

                    await this.NavigateTo(tf);

                    break;

                default:

                    this.LastMessage = message.RawMessageData.Message.Text;

                    break;
            }


        }


        public override async Task Render(MessageResult message)
        {
            if (message.Command == "reply")
            {

                await this.Device.Send("Letzte Nachricht: " + this.LastMessage);

            }

        }

    }
}
