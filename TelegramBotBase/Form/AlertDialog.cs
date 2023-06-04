using TelegramBotBase.Attributes;

namespace TelegramBotBase.Form;

/// <summary>
///     A simple prompt dialog with one ok Button
/// </summary>
[IgnoreState]
public class AlertDialog : ConfirmDialog
{
    public AlertDialog(string message, string buttonText) : base(message)
    {
        Buttons.Add(new ButtonBase(buttonText, "ok"));
        ButtonText = buttonText;
    }

    public string ButtonText { get; set; }
}