using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBotBase.Args
{
    public class MessageSentEventArgs
    {
        public int MessageId
        {
            get
            {
                return this.Message.MessageId;
            }
        }

        public Message Message { get; set; }

        public MessageSentEventArgs(Message message)
        {
            this.Message = message;
        }


    }
}
