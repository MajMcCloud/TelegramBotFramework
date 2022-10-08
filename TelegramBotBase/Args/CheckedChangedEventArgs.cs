using System;
using TelegramBotBase.Controls.Hybrid;

namespace TelegramBotBase.Args;

public class CheckedChangedEventArgs : EventArgs
{
    public CheckedChangedEventArgs()
    {
    }

    public CheckedChangedEventArgs(ButtonRow row, int index, bool @checked)
    {
        Row = row;
        Index = index;
        Checked = @checked;
    }

    /// <summary>
    ///     Contains the index of the row where the button is inside.
    ///     Contains -1 when it is a layout button or not found.
    /// </summary>
    public int Index { get; set; }


    /// <summary>
    ///     Contains all buttons within this row, excluding the checkbox.
    /// </summary>
    public ButtonRow Row { get; set; }


    /// <summary>
    ///     Contains the new checked status of the row.
    /// </summary>
    public bool Checked { get; set; }
}