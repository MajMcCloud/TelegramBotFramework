using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramBotBase.Sessions;

public static class Extensions
{
    public static Task Dispatch(this DeviceSession ds, Func<ITelegramBotClient, Task> request)
    {
        return ds.Dispatcher.Dispatch(ds, request);
    }

    public static Task<T> Dispatch<T>(this DeviceSession ds, Func<ITelegramBotClient, Task<T>> request)
    {
        return ds.Dispatcher.Dispatch(ds, request);
    }
}