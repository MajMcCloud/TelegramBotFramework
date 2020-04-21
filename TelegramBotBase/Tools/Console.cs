using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TelegramBotBase.Tools
{
    public static class Console
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        static List<Action> Actions = new List<Action>();

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        static Console()
        {

        }

        public static void SetHandler(Action action)
        {
            Actions.Add(action);

            if (_handler != null)
                return;

            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);
        }

        private static bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:

                    foreach (var a in Actions)
                    {
                        a();
                    }

                    return false;

                default:
                    return false;
            }
        }

    }
}
