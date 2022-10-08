using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotBase.Form;

/// <summary>
///     Base class for button handling
/// </summary>
public class TagButtonBase : ButtonBase
{
    public TagButtonBase()
    {
    }

    public TagButtonBase(string text, string value, string tag)
    {
        Text = text;
        Value = value;
        Tag = tag;
    }

    public string Tag { get; set; }


    /// <summary>
    ///     Returns an inline Button
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    public override InlineKeyboardButton ToInlineButton(ButtonForm form)
    {
        var id = form.DependencyControl != null ? form.DependencyControl.ControlId + "_" : "";

        return InlineKeyboardButton.WithCallbackData(Text, id + Value);
    }


    /// <summary>
    ///     Returns a KeyBoardButton
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    public override KeyboardButton ToKeyboardButton(ButtonForm form)
    {
        return new KeyboardButton(Text);
    }
}