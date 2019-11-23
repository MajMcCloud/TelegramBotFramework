using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBaseTest.Tests.Register.Steps
{
    public class Step3 : AutoCleanForm
    {
        public Data UserData { get; set; }

        public async override Task Load(MessageResult message)
        {
            if (message.Handled)
                return;

            if (message.MessageText.Trim() == "")
                return;

            if (this.UserData.EMail == null)
            {
                this.UserData.EMail = message.MessageText;
                return;
            }
        }

        public async override Task Action(MessageResult message)
        {
            await message.ConfirmAction();

            switch (message.RawData)
            {
                case "back":

                    var start = new Start();

                    await this.NavigateTo(start);

                    break;

            }

        }

        public async override Task Render(MessageResult message)
        {
            if (this.UserData.EMail == null)
            {
                await this.Device.Send("Please sent your email:");
                return;
            }

            message.Handled = true;

            String s = "";

            s += "Firstname: " + this.UserData.Firstname + "\r\n";
            s += "Lastname: " + this.UserData.Lastname + "\r\n";
            s += "E-Mail: " + this.UserData.EMail + "\r\n";

            ButtonForm bf = new ButtonForm();
            bf.AddButtonRow(new ButtonBase("Back", "back"));

            await this.Device.Send("Your details:\r\n" + s, bf);
        }

    }
}
