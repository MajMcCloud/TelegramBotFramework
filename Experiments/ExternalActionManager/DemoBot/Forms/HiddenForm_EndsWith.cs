using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace DemoBot.Forms
{
    public class HiddenForm_EndsWith : AutoCleanForm
    {
        public String value {  get; set; }

        public HiddenForm_EndsWith() {

            DeleteMode = TelegramBotBase.Enums.EDeleteMode.OnLeavingForm;
            DeleteSide = TelegramBotBase.Enums.EDeleteSide.Both;
        }

        public override async Task Action(MessageResult message)
        {
            if (message.RawData != "start")
            {
                return;
            }

            await message.ConfirmAction("Lets go");

            message.Handled = true;

            var st = new StartForm();

            await NavigateTo(st);
        }

        public override async Task Render(MessageResult message)
        {

            var bf = new ButtonForm();

            bf.AddButtonRow("Goto Start", "start");

            value = value.Replace("_", "\\_");

            await Device.Send($"Welcome to Hidden ends with form\n\nThe given value is {value}", bf);

        }

    }
}
