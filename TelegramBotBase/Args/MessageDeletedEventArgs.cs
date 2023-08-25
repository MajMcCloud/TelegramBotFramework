namespace TelegramBotBase.Args;

public class MessageDeletedEventArgs
{
    public MessageDeletedEventArgs(int messageId)
    {
        MessageId = messageId;
    }

    public int MessageId { get; set; }
}