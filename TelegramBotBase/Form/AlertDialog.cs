using TelegramBotBase.Attributes;

namespace TelegramBotBase.Form
{
    /// <summary>
    /// A simple prompt dialog with one ok Button
    /// </summary>
    [IgnoreState]
    public class AlertDialog : ConfirmDialog
    {
        public string ButtonText { get; set; }

        public AlertDialog(string message, string buttonText) : base(message)
        {
            Buttons.Add(new ButtonBase(buttonText, "ok"));
            this.ButtonText = buttonText;

        }

    }
}
