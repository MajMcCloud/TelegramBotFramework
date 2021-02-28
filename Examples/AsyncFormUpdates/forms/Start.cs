using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace AsyncFormUpdates.forms
{
    public class Start : AutoCleanForm
    {


        public override async Task Action(MessageResult message)
        {
            await message.ConfirmAction("");

            switch (message.RawData ?? "")
            {
                case "async":

                    var afe = new AsyncFormEdit();
                    await NavigateTo(afe);


                    break;

                case "async_del":

                    var afu = new AsyncFormUpdate();
                    await NavigateTo(afu);


                    break;
            }

        }

        public override async Task Render(MessageResult message)
        {
            var bf = new ButtonForm();

            bf.AddButtonRow("Open Async Form with AutoCleanupForm", "async_del");

            bf.AddButtonRow("Open Async Form with Edit", "async");

            await Device.Send("Choose your option", bf);
        }

    }
}
