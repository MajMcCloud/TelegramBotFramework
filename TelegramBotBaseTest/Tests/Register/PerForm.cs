using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBaseTest.Tests.Register
{
    public class PerForm : AutoCleanForm
    {
        public String EMail { get; set; }

        public String Firstname { get; set; }

        public String Lastname { get; set; }

        public async override Task Load(MessageResult message)
        {
            if (message.MessageText.Trim() == "")
                return;

            if (this.Firstname == null)
            {
                this.Firstname = message.MessageText;
                return;
            }

            if (this.Lastname == null)
            {
                this.Lastname = message.MessageText;
                return;
            }

            if (this.EMail == null)
            {
                this.EMail = message.MessageText;
                return;
            }

        }

        public async override Task Action(MessageResult message)
        {
            var call = message.GetData<CallbackData>();

            await message.ConfirmAction();

            if (call == null)
                return;

            switch (call.Value)
            {
                case "back":

                    var start = new Start();

                    await this.NavigateTo(start);

                    break;

            }


        }

        public async override Task Render(MessageResult message)
        {
            if (this.Firstname == null)
            {
                await this.Device.Send("Please sent your firstname:");
                return;
            }

            if (this.Lastname == null)
            {
                await this.Device.Send("Please sent your lastname:");
                return;
            }

            if (this.EMail == null)
            {
                await this.Device.Send("Please sent your email address:");
                return;
            }


            String s = "";

            s += "Firstname: " + this.Firstname + "\r\n";
            s += "Lastname: " + this.Lastname + "\r\n";
            s += "E-Mail: " + this.EMail + "\r\n";

            ButtonForm bf = new ButtonForm();
            bf.AddButtonRow(new ButtonBase("Back", new CallbackData("a", "back").Serialize()));

            await this.Device.Send("Your details:\r\n" + s, bf);
        }


    }
}
