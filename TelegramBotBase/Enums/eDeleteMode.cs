namespace TelegramBotBase.Enums;

public enum EDeleteMode
{
    /// <summary>
    ///     Don't delete any message.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Delete messages on every callback/action.
    /// </summary>
    OnEveryCall = 1,

    /// <summary>
    ///     Delete on leaving this form.
    /// </summary>
    OnLeavingForm = 2
}