using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TelegramBotBase.Tools;

public static class Console
{
    private static EventHandler __handler;

    private static readonly List<Action> __actions = new();

    static Console()
    {
    }

    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

    public static void SetHandler(Action action)
    {
        __actions.Add(action);

        if (__handler != null)
        {
            return;
        }

        __handler += Handler;
        SetConsoleCtrlHandler(__handler, true);
    }

    private static bool Handler(CtrlType sig)
    {
        switch (sig)
        {
            case CtrlType.CtrlCEvent:
            case CtrlType.CtrlLogoffEvent:
            case CtrlType.CtrlShutdownEvent:
            case CtrlType.CtrlCloseEvent:

                foreach (var a in __actions)
                {
                    a();
                }

                return false;

            default:
                return false;
        }
    }

    private delegate bool EventHandler(CtrlType sig);

    private enum CtrlType
    {
        CtrlCEvent = 0,
        CtrlBreakEvent = 1,
        CtrlCloseEvent = 2,
        CtrlLogoffEvent = 5,
        CtrlShutdownEvent = 6
    }
}