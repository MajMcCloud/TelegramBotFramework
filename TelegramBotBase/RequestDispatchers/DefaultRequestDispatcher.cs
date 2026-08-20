using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using TelegramBotBase.Base;
using TelegramBotBase.Constants;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Tools.RateLimiters;

namespace TelegramBotBase.RequestDispatchers;

/// <summary>
/// Base dispatcher that sends Telegram Bot API requests through a concurrency limiter
/// and a global pause gate, retrying automatically on 429 responses
/// </summary>
public class DefaultRequestDispatcher : IRequestDispatcher
{
    protected readonly RequestDispatcherSettings Settings;
    protected readonly MessageClient Client;
    protected readonly SemaphoreSlim ConcurrencyLimiter;
    protected readonly PauseGate PauseGate = new();
    
    /// <summary>Initializes the dispatcher and sizes the concurrency limiter from settings</summary>
    /// <param name="settings">Throttling and retry configuration</param>
    /// <param name="client">Telegram message client used to send requests</param>
    public DefaultRequestDispatcher(RequestDispatcherSettings settings, MessageClient client)
    {
        Settings = settings;
        Client = client;
        ConcurrencyLimiter = new SemaphoreSlim(Settings.MaxConcurrentRequests, Settings.MaxConcurrentRequests);
    }

    /// <summary>Executes a Telegram API request with no return value, with throttling and retry</summary>
    /// <param name="ds">Device session the request is being sent for</param>
    /// <param name="request">The Telegram API call to execute</param>
    /// <param name="ct">Cancellation token</param>
    public virtual Task Dispatch(IDeviceSession ds, Func<ITelegramBotClient, Task> request,
        CancellationToken ct = default)
    {
        return Dispatch(ds, 
            async client =>
        {
            await request(client);
            return true;
        }, 
            ct);
    }

    /// <summary>
    /// Executes a Telegram API request, retrying on 429 up to <see cref="RequestDispatcherSettings.MaxRetryAttempts"/>
    /// times
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
        var numberOfRetry = 0;
        while (true)
        {
            TimeSpan retryAfter;

            await OccupySpot(ds, ct);
            await PauseGate.WaitIfPaused(ct);

            try
            {
                return await request(Client.TelegramClient);
            }
            catch (ApiRequestException ex)
            {
                if (!TryHandleException(ex, out retryAfter))
                {
                    throw;
                }
            }
            finally
            {
                ReleaseSpot(ds);
            }

            numberOfRetry++;
            if (numberOfRetry > Settings.MaxRetryAttempts)
            {
                break;
            }

            await ProcessRetryAfter(retryAfter, ct);
        }

        return default;
    }

    protected virtual async Task OccupySpot(IDeviceSession ds, CancellationToken ct = default)
    {
        await ConcurrencyLimiter.WaitAsync(ct);
    }

    protected virtual void ReleaseSpot(IDeviceSession ds)
    {
        ConcurrencyLimiter.Release();
    }

    protected virtual bool TryHandleException(ApiRequestException ex, out TimeSpan retryAfter)
    {
        retryAfter = TimeSpan.Zero;
        
        if (ex.ErrorCode != Http.TooManyRequests)
        {
            return false;
        }

        retryAfter = Settings.RespectRetryAfterHeader && ex.Parameters is { RetryAfter: not null }
            ? TimeSpan.FromSeconds(ex.Parameters.RetryAfter.Value)
            : Settings.FallbackRetryAfter;

        return true;
    }

    protected virtual async Task ProcessRetryAfter(TimeSpan retryAfter, CancellationToken ct = default)
    {
        if (Settings.GlobalPauseOn429)
        {
            PauseGate.TriggerPause(retryAfter);
            return;
        }
        
        await Task.Delay(retryAfter, ct);
    }
}