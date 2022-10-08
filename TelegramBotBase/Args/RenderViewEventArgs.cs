using System;

namespace TelegramBotBase.Args
{
    public class RenderViewEventArgs : EventArgs
    {
        public int CurrentView { get; set; }


        public RenderViewEventArgs(int viewIndex)
        {

            CurrentView = viewIndex;
        }


    }
}
