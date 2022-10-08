using System;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Base;

namespace TelegramBotBase.Args
{
    public class GroupChangedEventArgs : EventArgs
    {
        public MessageType Type { get; set; }

        public MessageResult OriginalMessage { get; set; }

        public GroupChangedEventArgs(MessageType type, MessageResult message)
        {
            Type = type;
            OriginalMessage = message;
        }



    }
}
