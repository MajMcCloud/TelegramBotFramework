using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotBase.Args;
using TelegramBotBase.Base;

namespace TelegramBotBase.Interfaces
{
    public interface IStateMachine
    {
        void SaveFormStates(SaveStatesEventArgs e);

        StateContainer LoadFormStates();
    }
}
