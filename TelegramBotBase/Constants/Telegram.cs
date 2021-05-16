using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Constants
{
    public static class Telegram
    {
        /// <summary>
        /// The maximum length of message text before the API throws an exception. (We will catch it before)
        /// </summary>
        public const int MaxMessageLength = 4096;

        public const int MaxInlineKeyBoardRows = 13;

        public const int MaxInlineKeyBoardCols = 8;

        public const int MaxReplyKeyboardRows = 25;

        public const int MaxReplyKeyboardCols = 12;

        public const int MessageDeletionsPerSecond = 30;

    }
}
