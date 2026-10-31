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
/// Base dispatcher that sends Telegram Bot API requests through a concurrency limiter
/// and a global pause gate, retrying automatically on 429 responses. Requests that have to wait
/// are served by <see cref="RequestPriority"/> first, then in arrival order
/// </summary>
public class DefaultRequestDispatcher : IRequestDispatcher
{
    protected readonly RequestDispatcherSettings Settings;
    protected readonly MessageClient Client;
    protected readonly SemaphoreSlim ConcurrencyLimiter;
    protected readonly PauseGate PauseGate = new();

    protected readonly DeviceSession GlobalSession;

    private readonly PrioritySlotQueue _slotQueue;

    /// <summary>Initializes the dispatcher and sizes the concurrency limiter from settings</summary>
    /// <param name="settings">Throttling and retry configuration</param>
    /// <param name="client">Telegram message client used to send requests</param>
    public DefaultRequestDispatcher(RequestDispatcherSettings settings, MessageClient client)
    {
        Settings = settings;
        Client = client;
        ConcurrencyLimiter = new SemaphoreSlim(Settings.MaxConcurrentRequests, Settings.MaxConcurrentRequests);

        _slotQueue = new PrioritySlotQueue(TryOccupySpot, Settings.PriorityAgingInterval);

        GlobalSession = new DeviceSession(this)
        {
            DeviceId = 0
        };
    }

    /// <summary>Executes a Telegram API request with no return value, with throttling and retry</summary>
    /// <param name="ds">Device session the request is being sent for</param>
    /// <param name="request">The Telegram API call to execute</param>
    /// <param name="ct">Cancellation token</param>
    /// <param name="priority">Order in which this request is served relative to other waiting requests</param>
    public virtual Task Dispatch(IDeviceSession ds, Func<ITelegramBotClient, Task> request,
        CancellationToken ct = default, RequestPriority priority = RequestPriority.Normal)
    {
        return Dispatch(ds, 
            async client =>
        {
            await request(client);
            return true;
        }, 
            ct, priority);
    }

    /// <summary>
    /// Executes a Telegram API request, retrying on 429 up to <see cref="RequestDispatcherSettings.MaxRetryAttempts"/>
    /// times
    /// </summary>
    /// <typeparam name="T">Return type of the Telegram API call</typeparam>
    /// <param name="ds">Device session the request is being sent for</param>
    /// <param name="request">The Telegram API call to execute</param>
    /// <param name="ct">Cancellation token</param>
    /// <param name="priority">Order in which this request is served relative to other waiting requests</param>
    /// <returns>The result of <paramref name="request"/>, or <c>default</c> if all retries are exhausted</returns>
    /// <exception cref="ApiRequestException">Thrown on error code not equal to <see cref="Http.TooManyRequests"/></exception>
    public virtual async Task<T> Dispatch<T>(IDeviceSession ds, Func<ITelegramBotClient, Task<T>> request,
        CancellationToken ct = default, RequestPriority priority = RequestPriority.Normal)
    {
        // One ticket per logical request, so a retry after 429 keeps its place in the queue
        var ticket = _slotQueue.CreateTicket(priority);
        var numberOfRetry = 0;
        while (true)
        {
            TimeSpan retryAfter;

            await _slotQueue.WaitForSlot(ds, ticket, ct);

            try
            {
                await PauseGate.WaitIfPaused(ct);
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
                SignalSlotAvailable();
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

    /// <summary>
    /// Tries to take a dispatch slot for <paramref name="ds"/> without waiting. A slot that was acquired
    /// is given back through <see cref="ReleaseSpot"/>. Called by the scheduler under its lock, so it must not block
    /// </summary>
    protected virtual SlotAttempt TryOccupySpot(IDeviceSession ds)
    {
        return ConcurrencyLimiter.Wait(0) ? SlotAttempt.Acquired : SlotAttempt.BlockedGlobally;
    }

    protected virtual void ReleaseSpot(IDeviceSession ds)
    {
        ConcurrencyLimiter.Release();
    }

    /// <summary>
    /// Lets queued requests re-check for a free slot. Call it whenever a limit frees up outside of
    /// <see cref="ReleaseSpot"/>, e.g. after a rate-limit cooldown ends
    /// </summary>
    protected void SignalSlotAvailable()
    {
        _slotQueue.Drain();
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

    public virtual async Task Dispatch(Func<ITelegramBotClient, Task> request, CancellationToken ct = default,
        RequestPriority priority = RequestPriority.Normal)
    {
        await Dispatch(GlobalSession, request, ct, priority);
    }

    public virtual async Task<T> Dispatch<T>(Func<ITelegramBotClient, Task<T>> request, CancellationToken ct = default,
        RequestPriority priority = RequestPriority.Normal)
    {
        return await Dispatch(GlobalSession, request, ct, priority);
    }
}