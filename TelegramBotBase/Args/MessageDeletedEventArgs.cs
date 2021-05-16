using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBotBase.Args
{
    public class MessageDeletedEventArgs
    {
        public int MessageId
        {
            get;set;
        }

        public MessageDeletedEventArgs(int messageId)
        {
            this.MessageId = messageId;
        }

    }
}
