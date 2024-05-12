using System;
using Telegram.Bot.Exceptions;

namespace TelegramBotBase.Exceptions;

public sealed class CallbackDataTooLongException : Exception
{
    static ApiRequestException _innerException = new Telegram.Bot.Exceptions.ApiRequestException("Bad Request: BUTTON_DATA_INVALID", 400);

    static String _message = $"You have exceeded the maximum {Constants.Telegram.MaxCallBackDataBytes} bytes of callback data.\r\nThis is a pre-sending message from the TelegramBotBase framework.\r\nread more: https://core.telegram.org/bots/api#inlinekeyboardbutton";

    static String _message_with_bytes = $"You have exceeded the maximum {Constants.Telegram.MaxCallBackDataBytes} bytes of callback data with @current_callback_bytes@ bytes.\r\nThis is a pre-sending message from the TelegramBotBase framework.\r\nread more: https://core.telegram.org/bots/api#inlinekeyboardbutton";

    public CallbackDataTooLongException() : base(_message, _innerException)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="current_callback_bytes">The amount of callback bytes generated.</param>
    public CallbackDataTooLongException(int current_callback_bytes) : base(getMessage(current_callback_bytes), _innerException)
    {

    }

    static String getMessage(int current_callback_bytes = -1)
    {
        if (current_callback_bytes == -1)
            return _message;

        return _message_with_bytes.Replace("@current_callback_bytes@", current_callback_bytes.ToString());
    }
}