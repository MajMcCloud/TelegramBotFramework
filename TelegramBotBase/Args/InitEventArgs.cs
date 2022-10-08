using System;

namespace TelegramBotBase.Args
{
    public class InitEventArgs : EventArgs
    {
        public object[] Args { get; set; }

        public InitEventArgs(params object[] args)
        {
            Args = args;
        }
    }
}
