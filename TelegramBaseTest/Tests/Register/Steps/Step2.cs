using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBaseTest.Tests.Register.Steps
{
    public class Step2 : AutoCleanForm
    {
        public Data UserData { get; set; }


        public async override Task Load(MessageResult message)
        {
            if (message.Handled)
                return;

            if (message.MessageText.Trim() == "")
                return;

            if (this.UserData.Lastname == null)
            {
                this.UserData.Lastname = message.MessageText;
                return;
            }
        }


        public async override Task Render(MessageResult message)
        {
            if (this.UserData.Lastname == null)
            {
                await this.Device.Send("Please sent your lastname:");
                return;
            }

            message.Handled = true;

            var step3 = new Step3();

            step3.UserData = this.UserData;

            await this.NavigateTo(step3);
        }

    }
}
