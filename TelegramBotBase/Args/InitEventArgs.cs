using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Args
{
    public class InitEventArgs : EventArgs
    {
        public object[] Args { get; set; }

        public InitEventArgs(params object[] args)
        {
            this.Args = args;
        }
    }
}
