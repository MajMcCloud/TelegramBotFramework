using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.RequestDispatchers;

public interface IRequestDispatcher
{
    Task Dispatch(IDeviceSession ds, Func<ITelegramBotClient, Task> request, CancellationToken ct = default);
    Task<T> Dispatch<T>(IDeviceSession ds, Func<ITelegramBotClient, Task<T>> request, CancellationToken ct = default);


    Task Dispatch(Func<ITelegramBotClient, Task> request, CancellationToken ct = default);
    Task<T> Dispatch<T>(Func<ITelegramBotClient, Task<T>> request, CancellationToken ct = default);
}