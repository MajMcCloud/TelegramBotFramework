using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Form.Navigation;

namespace TelegramBotBaseTest.Tests.Navigation
{
    public class Start : FormBase
    {

        Message msg = null;

        public Start()
        {
            this.Closed += Start_Closed;
        }

        private async Task Start_Closed(object sender, EventArgs e)
        {
            if (msg == null)
                return;

            await Device.DeleteMessage(msg);
        }

        public override async Task Load(MessageResult message)
        {



        }

        public override async Task Action(MessageResult message)
        {
            if (message.Handled)
                return;

            await message.ConfirmAction();

            switch (message.RawData)
            {
                case "yes":

                    message.Handled = true;

                    //Create navigation controller and navigate to it, keep the current form as root form so we can get back to here later
                    var nc = new CustomController(this);

                    var f1 = new Form1();

                    await NavigateTo(nc);

                    await nc.PushAsync(f1);


                    break;
                case "no":

                    message.Handled = true;

                    var mn = new Menu();

                    await NavigateTo(mn);


                    break;
            }

        }

        public override async Task Render(MessageResult message)
        {
            var bf = new ButtonForm();

            bf.AddButtonRow("Yes", "yes");
            bf.AddButtonRow("No", "no");

            msg = await Device.Send("Open controller?", bf);


        }

    }
}
