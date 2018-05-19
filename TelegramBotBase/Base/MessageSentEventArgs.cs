using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBotBase.Base
{
    public class MessageSentEventArgs
    {
        public int MessageId { get; set; }

        public Message Message { get; set; }

        public MessageSentEventArgs()
        {

        }

        public MessageSentEventArgs(int MessageId, Message message)
        {
            this.MessageId = MessageId;
            this.Message = message;
        }


    }
}
