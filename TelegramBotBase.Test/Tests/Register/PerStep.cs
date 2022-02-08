using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBaseTest.Tests.Register
{
    public class PerStep: AutoCleanForm
    {

        public async override Task Action(MessageResult message)
        {
            await message.ConfirmAction();

            switch (message.RawData)
            {
                case "start":

                    var step1 = new Steps.Step1();

                    await this.NavigateTo(step1);

                    break;
                case "back":

                    var start = new Start();

                    await this.NavigateTo(start);

                    break;

            }
        }

        public async override Task Render(MessageResult message)
        {
            ButtonForm bf = new ButtonForm();
            bf.AddButtonRow(new ButtonBase("Goto Step 1", "start"));
            bf.AddButtonRow(new ButtonBase("Back", "back"));

            await this.Device.Send("Register Steps", bf);
        }
    }
}
