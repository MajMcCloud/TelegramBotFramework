using System;

namespace TelegramBotBase.Exceptions;

public sealed class MessageTooLongException : Exception
{
    public MessageTooLongException(int length)
    {
        Length = length;
    }

    public int Length { get; set; }

    public override string Message =>
        $"You have exceeded the maximum of message length by {Length}/{Constants.Telegram.MaxMessageLength}";
}
