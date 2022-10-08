using System;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Form;

namespace TelegramBotBase.Args;

/// <summary>
///     Button get clicked event
/// </summary>
public class ButtonClickedEventArgs : EventArgs
{
    public ButtonClickedEventArgs()
    {
    }

    public ButtonClickedEventArgs(ButtonBase button)
    {
        Button = button;
        Index = -1;
    }

    public ButtonClickedEventArgs(ButtonBase button, int index)
    {
        Button = button;
        Index = index;
    }

    public ButtonClickedEventArgs(ButtonBase button, int index, ButtonRow row)
    {
        Button = button;
        Index = index;
        Row = row;
    }

    public ButtonBase Button { get; set; }

    public int Index { get; set; }

    public object Tag { get; set; }

    public ButtonRow Row { get; set; }
}