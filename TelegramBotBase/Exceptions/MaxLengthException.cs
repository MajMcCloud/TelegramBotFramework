using System;

namespace TelegramBotBase.Exceptions;

public class MaxLengthException : Exception
{
    public MaxLengthException(int length) : base(
        $"Your messages with a length of {length} is too long for telegram. Actually is {Constants.Telegram.MaxMessageLength} characters allowed. Please split it.")
    {
    }
}