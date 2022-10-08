using System;
using Telegram.Bot.Types;

namespace TelegramBotBase.Args;

public class MessageSentEventArgs : EventArgs
{
    public MessageSentEventArgs(Message message, Type origin)
    {
        Message = message;
        Origin = origin;
    }

    public int MessageId => Message.MessageId;

    public Message Message { get; set; }

    /// <summary>
    ///     Contains the element, which has called the method.
    /// </summary>
    public Type Origin { get; set; }
}