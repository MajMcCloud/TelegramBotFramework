using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBaseTest.Tests
{
    public class ButtonTestForm : AutoCleanForm
    {

        public override async Task Opened()
        {
            await this.Device.Send("Hello world! (Click 'back' to get back to Start)");
        }

        public override async Task Action(MessageResult message)
        {

            var call = message.GetData<CallbackData>();

            await message.ConfirmAction();


            if (call == null)
                return;

            switch (call.Value)
            {
                case "button1":

                    await this.Device.Send("Button 1 pressed");

                    break;

                case "button2":

                    await this.Device.Send("Button 2 pressed");

                    break;

                case "button3":

                    await this.Device.Send("Button 3 pressed");

                    break;

                case "button4":

                    await this.Device.Send("Button 4 pressed");

                    break;

                case "back":

                    var st = new Start();

                    await st.Init();

                    await this.NavigateTo(st);

                    break;
            }


        }


        public override async Task Render(MessageResult message)
        {

            ButtonForm btn = new ButtonForm();

            btn.AddButtonRow(new ButtonBase("Button 1", new CallbackData("a", "button1").Serialize()), new ButtonBase("Button 2", new CallbackData("a", "button2").Serialize()));

            btn.AddButtonRow(new ButtonBase("Button 3", new CallbackData("a", "button3").Serialize()), new ButtonBase("Button 4", new CallbackData("a", "button4").Serialize()));

            btn.AddButtonRow(new ButtonBase("Back", new CallbackData("a", "back").Serialize()));

            await this.Device.Send("Click a button", btn);


        }
    }
}


