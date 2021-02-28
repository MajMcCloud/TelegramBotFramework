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

        public int Index { get; set; }


        public ButtonClickedEventArgs()
        {

        }

        public ButtonClickedEventArgs(ButtonBase button)
        {
            this.Button = button;
            this.Index = -1;
        }

        public ButtonClickedEventArgs(ButtonBase button, int Index)
        {
            this.Button = button;
            this.Index = Index;
        }

    }
}
