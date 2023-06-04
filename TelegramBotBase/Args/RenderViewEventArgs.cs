using System;

namespace TelegramBotBase.Args;

public class RenderViewEventArgs : EventArgs
{
    public RenderViewEventArgs(int viewIndex)
    {
        CurrentView = viewIndex;
    }

    public int CurrentView { get; set; }
}