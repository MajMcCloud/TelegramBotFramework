using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotBase.Form
{
    /// <summary>
    /// Base class for button handling
    /// </summary>
    public class ButtonBase
    {
        public String Text { get; set; }

        public String Value { get; set; }

        public String Url { get; set; }

        public ButtonBase()
        {

        }

        public ButtonBase(String Text, String Value, String Url = null)
        {
            this.Text = Text;
            this.Value = Value;
            this.Url = Url;
        }


        public InlineKeyboardButton ToInlineButton()
        {
            if (this.Url == null)
            {
                return InlineKeyboardButton.WithCallbackData(this.Text, this.Value);
            }

            var ikb = new InlineKeyboardButton();

            ikb.Text = this.Text;
            ikb.Url = this.Url;

            return ikb;

        }

    }
}
