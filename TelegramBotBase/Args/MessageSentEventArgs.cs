using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBotBase.Args
{
    public class MessageSentEventArgs : EventArgs
    {
        public int MessageId
        {
            get
            {
                return this.Message.MessageId;
            }
        }

        public Message Message { get; set; }

        /// <summary>
        /// Contains the element, which has called the method.
        /// </summary>
        public Type Origin { get; set; }


        public MessageSentEventArgs(Message message, Type Origin)
        {
            this.Message = message;
            this.Origin = Origin;
        }


    }
}
