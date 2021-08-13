using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Controls.Hybrid;
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

        public object Tag { get; set; }

        public ButtonRow Row { get; set; }


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

        public ButtonClickedEventArgs(ButtonBase button, int Index, ButtonRow row)
        {
            this.Button = button;
            this.Index = Index;
            this.Row = row;
        }
    }
}
