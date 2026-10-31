using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.RequestDispatchers;

/// <summary>
/// Controls how outgoing Telegram Bot API requests are executed: throttling, concurrency,
/// retries and the order in which waiting requests are served
/// </summary>
public interface IRequestDispatcher
{
    /// <summary>Executes a Telegram API request with no return value on behalf of a device session</summary>
    /// <param name="ds">Device session the request is sent for; used to apply per-chat and per-group limits</param>
    /// <param name="request">The Telegram API call to execute</param>
    /// <param name="ct">Cancellation token</param>
    /// <param name="priority">Order in which waiting requests are served; implementations without queueing may ignore it</param>
    /// <returns>A task that completes once the request has been executed</returns>
    Task Dispatch(IDeviceSession ds, Func<ITelegramBotClient, Task> request, CancellationToken ct = default,
        RequestPriority priority = RequestPriority.Normal);

    /// <summary>Executes a Telegram API request on behalf of a device session and returns its result</summary>
    /// <typeparam name="T">Return type of the Telegram API call</typeparam>
    /// <param name="ds">Device session the request is sent for; used to apply per-chat and per-group limits</param>
    /// <param name="request">The Telegram API call to execute</param>
    /// <param name="ct">Cancellation token</param>
    /// <param name="priority">Order in which waiting requests are served; implementations without queueing may ignore it</param>
    /// <returns>The result of <paramref name="request"/>; implementations may return <c>default</c> when retries are exhausted</returns>
    Task<T> Dispatch<T>(IDeviceSession ds, Func<ITelegramBotClient, Task<T>> request, CancellationToken ct = default,
        RequestPriority priority = RequestPriority.Normal);

    /// <summary>Executes a Telegram API request with no return value that is not bound to a specific chat</summary>
    /// <param name="request">The Telegram API call to execute</param>
    /// <param name="ct">Cancellation token</param>
    /// <param name="priority">Order in which waiting requests are served; implementations without queueing may ignore it</param>
    /// <returns>A task that completes once the request has been executed</returns>
    Task Dispatch(Func<ITelegramBotClient, Task> request, CancellationToken ct = default,
        RequestPriority priority = RequestPriority.Normal);

    /// <summary>Executes a Telegram API request that is not bound to a specific chat and returns its result</summary>
    /// <typeparam name="T">Return type of the Telegram API call</typeparam>
    /// <param name="request">The Telegram API call to execute</param>
    /// <param name="ct">Cancellation token</param>
    /// <param name="priority">Order in which waiting requests are served; implementations without queueing may ignore it</param>
    /// <returns>The result of <paramref name="request"/>; implementations may return <c>default</c> when retries are exhausted</returns>
    Task<T> Dispatch<T>(Func<ITelegramBotClient, Task<T>> request, CancellationToken ct = default,
        RequestPriority priority = RequestPriority.Normal);
}
