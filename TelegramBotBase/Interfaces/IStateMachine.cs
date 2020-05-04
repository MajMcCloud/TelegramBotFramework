using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Interfaces
{
    public interface IStateMachine
    {
        Type FallbackStateForm { get; }

        void SaveFormStates(SaveStatesEventArgs e);

        StateContainer LoadFormStates();
    }
}
