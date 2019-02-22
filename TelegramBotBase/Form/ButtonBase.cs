using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Form
{
    /// <summary>
    /// Base class for button handling
    /// </summary>
    public class ButtonBase
    {
        public String Text { get; set; }

        public String Value { get; set; }

        public ButtonBase()
        {

        }

        public ButtonBase(String Text, String Value)
        {
            this.Text = Text;
            this.Value = Value;
        }

    }
}
