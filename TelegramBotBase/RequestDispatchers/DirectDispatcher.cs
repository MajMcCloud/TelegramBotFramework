using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using TelegramBotBase.Base;
using TelegramBotBase.Constants;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;
using TelegramBotBase.Tools.RateLimiters;

namespace TelegramBotBase.RequestDispatchers;

/// <summary>
/// Base dispatcher that sends Telegram Bot API requests with no throttling or retry logic
/// </summary>
public class DirectDispatcher : IRequestDispatcher
{
    protected readonly MessageClient Client;

    protected readonly DeviceSession GlobalSession;

    /// <summary>Initializes the dispatcher and sizes the concurrency limiter from settings</summary>
    /// <param name="settings">Throttling and retry configuration</param>
    /// <param name="client">Telegram message client used to send requests</param>
    public DirectDispatcher(MessageClient client)
    {
        Client = client;

        GlobalSession = new DeviceSession(this)
        {
            DeviceId = 0
        };
    }

    /// <summary>Executes a Telegram API request with no return value, with no throttling or retry logic</summary>
    /// <param name="ds">Device session the request is being sent for</param>
    /// <param name="request">The Telegram API call to execute</param>
    /// <param name="ct">Cancellation token</param>
    public virtual async Task Dispatch(IDeviceSession ds, Func<ITelegramBotClient, Task> request,
        CancellationToken ct = default)
    {
        await request(Client.TelegramClient);
    }


    /// <summary>
    /// Executes a Telegram API request with no throttling or retry logic
    /// </summary>
    /// <typeparam name="T">Return type of the Telegram API call</typeparam>
    /// <param name="ds">Device session the request is being sent for</param>
    /// <param name="request">The Telegram API call to execute</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The result of <paramref name="request"/>, or <c>default</c> if all retries are exhausted</returns>
    /// <exception cref="ApiRequestException">Thrown on error code not equal to <see cref="Http.TooManyRequests"/></exception>
    public virtual async Task<T> Dispatch<T>(IDeviceSession ds, Func<ITelegramBotClient, Task<T>> request,
        CancellationToken ct = default)
    {
        return await request(Client.TelegramClient);
    }

    /// <summary>
    /// Executes a Telegram API request with no return value, with no throttling or retry logic
    /// </summary>
    /// <param name="request">The Telegram API call to execute</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public virtual async Task Dispatch(Func<ITelegramBotClient, Task> request, CancellationToken ct = default)
    {
        await Dispatch(GlobalSession, request, ct);
    }

    /// <summary>
    /// Executes a Telegram API request with no throttling or retry logic
    /// </summary>
    /// <typeparam name="T">Return type of the Telegram API call</typeparam>
    /// <param name="request">The Telegram API call to execute</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The result of <paramref name="request"/>, or <c>default</c> if all retries are exhausted</returns>
    public virtual async Task<T> Dispatch<T>(Func<ITelegramBotClient, Task<T>> request, CancellationToken ct = default)
    {
        return await Dispatch(GlobalSession, request, ct);
    }
}