using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotBase.Args;
using TelegramBotBase.Base;

namespace TelegramBotBase.Interfaces
{
    /// <summary>
    /// Is used to save specific fields into a session state to survive restarts or unhandled exceptions and crashes.
    /// </summary>
    public interface IStateForm
    {

        void LoadState(LoadStateEventArgs e);

        void SaveState(SaveStateEventArgs e);

    }
}
