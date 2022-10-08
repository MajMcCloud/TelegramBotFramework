using Telegram.Bot.Types;

namespace TelegramBotBase.Args;

public class MessageReceivedEventArgs
{
    public MessageReceivedEventArgs(Message m)
    {
        Message = m;
    }

    public int MessageId => Message.MessageId;

    public Message Message { get; set; }
}