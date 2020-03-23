using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Base;

namespace TelegramBotBase.Args
{
    public class MemberChangeEventArgs : EventArgs
    {
        public List<User> Members { get; set; }

        public MessageType Type { get; set; }

        public MessageResult Result { get; set; }

        public MemberChangeEventArgs()
        {
            this.Members = new List<User>();

        }

        public MemberChangeEventArgs(MessageType type, MessageResult result, params User[] members)
        {
            this.Type = type;
            this.Result = result;
            this.Members = members.ToList();
        }





    }
}
