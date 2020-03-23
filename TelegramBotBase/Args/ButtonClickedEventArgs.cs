using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Form;

namespace TelegramBotBase.Args
{
    /// <summary>
    /// Button get clicked event
    /// </summary>
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
