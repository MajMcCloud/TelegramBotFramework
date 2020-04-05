using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TelegramBotBase.Base;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Args
{
    public class SaveStatesEventArgs
    {
        public StateContainer States { get; set; }


        public SaveStatesEventArgs(StateContainer states)
        {
            this.States = states;
        }
    }
}
