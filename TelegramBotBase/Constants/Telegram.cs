namespace TelegramBotBase.Constants;

public static class Telegram
{
    /// <summary>
    ///     The maximum length of message text before the API throws an exception. (We will catch it before)
    /// </summary>
    public const int MaxMessageLength = 4096;

    public const int MaxInlineKeyBoardRows = 13;

    public const int MaxInlineKeyBoardCols = 8;

    public const int MaxReplyKeyboardRows = 25;

    public const int MaxReplyKeyboardCols = 12;

    public const int MessageDeletionsPerSecond = 30;

    /// <summary>
    /// The maximum length of callback data. Will raise an exception of it exceeds it.
    /// </summary>
    public const int MaxCallBackDataBytes = 64;


    /// <summary>
    /// The slash constant which indicates a bot command.
    /// </summary>
    public const string BotCommandIndicator = "/";
}