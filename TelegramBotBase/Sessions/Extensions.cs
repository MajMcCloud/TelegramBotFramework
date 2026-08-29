using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Sessions;

public static class Extensions
{
    public static Task Dispatch(this IDeviceSession ds, Func<ITelegramBotClient, Task> request, CancellationToken ct = default )
    {
        return ds.Dispatcher.Dispatch(ds, request, ct);
    }

    public static Task<T> Dispatch<T>(this IDeviceSession ds, Func<ITelegramBotClient, Task<T>> request, CancellationToken ct = default)
    {
        return ds.Dispatcher.Dispatch(ds, request, ct);
    }
}