using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotBase.Args
{
    public class RenderViewEventArgs : EventArgs
    {
        public int CurrentView { get; set; }


        public RenderViewEventArgs(int ViewIndex)
        {

            CurrentView = ViewIndex;
        }


    }
}
