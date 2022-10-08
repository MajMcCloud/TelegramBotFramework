namespace TelegramBotBase.Enums;

public enum EDeleteSide
{
    /// <summary>
    ///     Delete only messages from this bot.
    /// </summary>
    BotOnly = 0,

    /// <summary>
    ///     Delete only user messages.
    /// </summary>
    UserOnly = 1,

    /// <summary>
    ///     Delete all messages in this context.
    /// </summary>
    Both = 2
}