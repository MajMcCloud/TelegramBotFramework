using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.RequestDispatchers;

public interface IRequestDispatcher
{
    /// <param name="priority">Order in which waiting requests are served; implementations without queueing may ignore it</param>
    Task Dispatch(IDeviceSession ds, Func<ITelegramBotClient, Task> request, CancellationToken ct = default,
        RequestPriority priority = RequestPriority.Normal);

    Task<T> Dispatch<T>(IDeviceSession ds, Func<ITelegramBotClient, Task<T>> request, CancellationToken ct = default,
        RequestPriority priority = RequestPriority.Normal);


    Task Dispatch(Func<ITelegramBotClient, Task> request, CancellationToken ct = default,
        RequestPriority priority = RequestPriority.Normal);

    Task<T> Dispatch<T>(Func<ITelegramBotClient, Task<T>> request, CancellationToken ct = default,
        RequestPriority priority = RequestPriority.Normal);
}