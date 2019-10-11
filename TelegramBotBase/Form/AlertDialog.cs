using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;

namespace TelegramBotBase.Form
{
    /// <summary>
    /// A simple prompt dialog with one ok Button
    /// </summary>
    public class AlertDialog : ConfirmDialog
    {
        public String ButtonText { get; set; }

        public AlertDialog(String Message, String ButtonText) : base(Message)
        {
            this.Buttons.Add(new ButtonBase(ButtonText, "ok"));
            this.ButtonText = ButtonText;

        }

    }
}
