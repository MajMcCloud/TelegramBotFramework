using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Base;

namespace TelegramBotBase.Args;

public class MemberChangeEventArgs : EventArgs
{
    public MemberChangeEventArgs()
    {
        Members = new List<User>();
    }

    public MemberChangeEventArgs(MessageType type, MessageResult result, params User[] members)
    {
        Type = type;
        Result = result;
        Members = members.ToList();
    }

    public List<User> Members { get; set; }

    public MessageType Type { get; set; }

    public MessageResult Result { get; set; }
}