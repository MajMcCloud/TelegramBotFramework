using Telegram.Bot.Types;

namespace TelegramBotBase.Args
{
    public class MessageReceivedEventArgs
    {
        public int MessageId => Message.MessageId;

        public Message Message { get; set; }

        public MessageReceivedEventArgs(Message m)
        {
            Message = m;
        }

    }
}
