using System;

namespace TelegramBotBase.Args;

public class InitEventArgs : EventArgs
{
    public InitEventArgs(params object[] args)
    {
        Args = args;
    }

    public object[] Args { get; set; }
}