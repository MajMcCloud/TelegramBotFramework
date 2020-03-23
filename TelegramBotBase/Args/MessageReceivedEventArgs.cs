using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBotBase.Args
{
    public class MessageReceivedEventArgs
    {
        public int MessageId
        {
            get
            {
                return this.Message.MessageId;
            }
        }

        public Message Message { get; set; }

        public MessageReceivedEventArgs(Message m)
        {
            this.Message = m;
        }

    }
}
