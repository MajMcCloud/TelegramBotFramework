using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Form;

namespace TelegramBotBase.Base
{
    public class ButtonClickedEventArgs : EventArgs
    {
        public ButtonBase Button { get; set; }


        public ButtonClickedEventArgs()
        {

        }

        public ButtonClickedEventArgs(ButtonBase button)
        {
            this.Button = button;
        }

    }
}
