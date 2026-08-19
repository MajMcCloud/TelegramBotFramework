using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.RequestDispatchers;

public interface IRequestDispatcher
{
    Task Dispatch(DeviceSession ds, Func<ITelegramBotClient, Task> request, CancellationToken ct = default);
    Task<T> Dispatch<T>(DeviceSession ds, Func<ITelegramBotClient, Task<T>> request, CancellationToken ct = default);
}