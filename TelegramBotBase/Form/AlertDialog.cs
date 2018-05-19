using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;

namespace TelegramBotBase.Form
{
    public class AlertDialog : PromptDialog
    {
        public String ButtonText { get; set; }

        public AlertDialog(String Message, String ButtonText, FormBase FormToOpen = null) : base(Message)
        {
            this.Buttons.Add(new ButtonBase(ButtonText, "ok"));
            this.ButtonText = ButtonText;

            if (FormToOpen != null)
                this.ButtonForms.Add("ok", FormToOpen);
        }

    }
}
