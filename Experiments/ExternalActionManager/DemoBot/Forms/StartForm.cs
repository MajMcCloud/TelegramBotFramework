using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace DemoBot.Forms
{
    internal class StartForm : AutoCleanForm
    {
        public StartForm()
        {

            DeleteMode = TelegramBotBase.Enums.EDeleteMode.OnLeavingForm;
            DeleteSide = TelegramBotBase.Enums.EDeleteSide.Both;

            Opened += StartForm_Opened;
        }

        private async Task StartForm_Opened(object sender, EventArgs e)
        {
            await Device.Send("Hey!\r\n\r\nChoose the /test command to get a message from outside.", disableNotification: true);
        }

        public override async Task Load(MessageResult message)
        {
            
            


        }

    }
}
