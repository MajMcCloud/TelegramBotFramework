using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Form;

namespace TelegramBotBase.Args
{
    public class CheckedChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Contains the index of the row where the button is inside. 
        /// Contains -1 when it is a layout button or not found.
        /// </summary>
        public int Index { get; set; }


        /// <summary>
        /// Contains all buttons within this row, excluding the checkbox.
        /// </summary>
        public ButtonRow Row { get; set; }


        /// <summary>
        /// Contains the new checked status of the row.
        /// </summary>
        public bool Checked { get; set; }


        public CheckedChangedEventArgs()
        {

        }

        public CheckedChangedEventArgs(ButtonRow row, int Index, bool Checked)
        {
            this.Row = row;
            this.Index = Index;
            this.Checked = Checked;
        }


    }
}
