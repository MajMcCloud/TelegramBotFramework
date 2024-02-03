using System;
using Telegram.Bot.Exceptions;

namespace TelegramBotBase.Exceptions;

public sealed class CallbackDataTooLongException : Exception
{
    //public override string Message =>
    //    $"You have exceeded the maximum {Constants.Telegram.MaxCallBackDataBytes} bytes.";

    static ApiRequestException _innerException = new Telegram.Bot.Exceptions.ApiRequestException("Bad Request: BUTTON_DATA_INVALID", 400);

    static String _message = $"You have exceeded the maximum {Constants.Telegram.MaxCallBackDataBytes} bytes of callback data.\r\nThis is a pre-sending message from the TelegramBotBase framework.\r\nread more: https://core.telegram.org/bots/api#inlinekeyboardbutton";

    public CallbackDataTooLongException() : base(_message, _innerException)
    {

    }


}
