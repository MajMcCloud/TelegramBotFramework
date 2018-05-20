using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBaseTest.Tests
{
    public class Start : FormBase
    {

        public override async Task Action(MessageResult message)
        {
            var call = message.GetData<CallbackData>();

            await message.ConfirmAction();


            if (call == null)
                return;

            switch(call.Value)
            {
                case "text":

                    var sf = new SimpleForm();

                    await sf.Init();

                    await this.NavigateTo(sf);

                    break;

                case "buttons":

                    var bf = new ButtonTestForm();

                    await bf.Init();

                    await this.NavigateTo(bf);

                    break;
            }


        }

        public override async Task Render(MessageResult message)
        {

            ButtonForm btn = new ButtonForm();

            btn.AddButtonRow(new ButtonBase("#1 Simple Text", new CallbackData("a", "text").Serialize()), new ButtonBase("#2 Button Test", new CallbackData("a", "buttons").Serialize()));


            await this.Device.Send("Choose your test:", btn);
        }


    }
}
