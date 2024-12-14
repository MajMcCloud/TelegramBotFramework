using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace FileWatcher.Forms
{
    public class Start : FormBase
    {

        public override async Task Load(MessageResult message)
        {

            await Device.Send("I'm here !");
        }
    }
}
