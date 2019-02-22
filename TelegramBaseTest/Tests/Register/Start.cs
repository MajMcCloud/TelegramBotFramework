using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBaseTest.Tests.Register
{
    public class Start : AutoCleanForm
    {
        public Start()
        {

        }

        public async override Task Action(MessageResult message)
        {
            var call = message.GetData<CallbackData>();

            await message.ConfirmAction();


            if (call == null)
                return;

            switch (call.Value)
            {
                case "form":

                    var form = new PerForm();

                    await this.NavigateTo(form);

                    break;
                case "step":

                    var step = new PerStep();

                    await this.NavigateTo(step);

                    break;
                case "backtodashboard":

                    var start = new Tests.Start();

                    await this.NavigateTo(start);

                    break;
            }


        }

        public async override Task Render(MessageResult message)
        {

            ButtonForm btn = new ButtonForm();

            btn.AddButtonRow(new ButtonBase("#4.1 Per Form", new CallbackData("a", "form").Serialize()));
            btn.AddButtonRow(new ButtonBase("#4.2 Per Step", new CallbackData("a", "step").Serialize()));
            btn.AddButtonRow(new ButtonBase("Back", new CallbackData("a", "backtodashboard").Serialize()));

            await this.Device.Send("Choose your test:", btn);


        }


    }
}
