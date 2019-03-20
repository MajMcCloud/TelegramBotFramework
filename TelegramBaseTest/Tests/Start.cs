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

                    await this.NavigateTo(sf);

                    break;

                case "buttons":

                    var bf = new ButtonTestForm();

                    await this.NavigateTo(bf);

                    break;

                case "progress":

                    var pf = new ProgressTest();

                    await this.NavigateTo(pf);

                    break;

                case "registration":

                    var reg = new Register.Start();

                    await this.NavigateTo(reg);

                    break;

                case "form1":

                    var form1 = new TestForm();

                    await this.NavigateTo(form1);

                    break;

                case "form2":

                    var form2 = new TestForm2();

                    await this.NavigateTo(form2);

                    break;
            }


        }

        public override async Task Render(MessageResult message)
        {

            ButtonForm btn = new ButtonForm();

            btn.AddButtonRow(new ButtonBase("#1 Simple Text", new CallbackData("a", "text").Serialize()), new ButtonBase("#2 Button Test", new CallbackData("a", "buttons").Serialize()));
            btn.AddButtonRow(new ButtonBase("#3 Progress Bar", new CallbackData("a", "progress").Serialize()));
            btn.AddButtonRow(new ButtonBase("#4 Registration Example", new CallbackData("a", "registration").Serialize()));

            btn.AddButtonRow(new ButtonBase("#5 Form1 Command", new CallbackData("a", "form1").Serialize()));

            btn.AddButtonRow(new ButtonBase("#6 Form2 Command", new CallbackData("a", "form2").Serialize()));

            await this.Device.Send("Choose your test:", btn);
        }


    }
}
