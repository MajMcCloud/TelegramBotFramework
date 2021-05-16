using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace AsyncFormUpdates.forms
{
    public class AsyncFormUpdate : AutoCleanForm
    {
        [SaveState]
        int counter = 0;


        public override async Task Load(MessageResult message)
        {
            counter++;
        }

        public override async Task Action(MessageResult message)
        {
            await message.ConfirmAction("");

            switch (message.RawData ?? "")
            {
                case "back":

                    var st = new Start();
                    await NavigateTo(st);

                    break;
            }
        }

        public override async Task Render(MessageResult message)
        {
            var bf = new ButtonForm();
            bf.AddButtonRow("Back", "back");

            await Device.Send($"Your current count is at: {counter}", bf, disableNotification: true);
        }


    }
}
