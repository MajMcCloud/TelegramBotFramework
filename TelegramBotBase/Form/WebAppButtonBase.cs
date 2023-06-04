using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotBase.Form
{
    public class WebAppButtonBase : ButtonBase
    {
        public WebAppInfo WebAppInfo { get; set; }

        public WebAppButtonBase()
        {

        }

        public WebAppButtonBase(String Text, WebAppInfo WebAppInfo)
        {
            this.Text = Text;
            this.WebAppInfo = WebAppInfo;
        }

        /// <summary>
        /// Returns an inline Button
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public override InlineKeyboardButton ToInlineButton(ButtonForm form)
        {
            return InlineKeyboardButton.WithWebApp(this.Text, this.WebAppInfo);
        }


        /// <summary>
        /// Returns a KeyBoardButton
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public override KeyboardButton ToKeyboardButton(ButtonForm form)
        {
            return KeyboardButton.WithWebApp(this.Text, this.WebAppInfo);
        }

    }
}
