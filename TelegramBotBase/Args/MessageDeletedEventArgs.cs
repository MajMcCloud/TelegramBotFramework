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
            MessageId = messageId;
        }

    }
}
