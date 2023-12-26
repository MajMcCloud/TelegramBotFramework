using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace TelegramBotBase.Base;

/// <summary>
///     Base class for message handling
/// </summary>
public class MessageClient
{
    private static readonly object EvOnMessageLoop = new();
    private static readonly object EvOnReceiveError = new();

    private static object __evOnMessage = new();

    private static object __evOnMessageEdit = new();

    private static object __evCallbackQuery = new();

    private CancellationTokenSource _cancellationTokenSource;

    public string ApiKey { get; }

    public ITelegramBotClient TelegramClient { get; set; }

    private EventHandlerList Events { get; } = new();

    /// <summary>
    ///    Indicates if all pending Telegram.Bot.Types.Updates should be thrown out before
    //     start polling. If set to true Telegram.Bot.Polling.ReceiverOptions.AllowedUpdates
    //     should be set to not null, otherwise Telegram.Bot.Polling.ReceiverOptions.AllowedUpdates
    //     will effectively be set to receive all Telegram.Bot.Types.Updates.
    /// </summary>
    public bool ThrowPendingUpdates { get; set; }

    public bool UseThreadPool { get; set; } = false;

    public int ThreadPool_WorkerThreads { get; set; } = 1;

    public int ThreadPool_IOThreads { get; set; } = 1;


    public MessageClient(string apiKey)
    {
        ApiKey = apiKey;
        TelegramClient = new TelegramBotClient(apiKey);

        Prepare();
    }

    public MessageClient(string apiKey, HttpClient proxy)
    {
        ApiKey = apiKey;
        TelegramClient = new TelegramBotClient(apiKey, proxy);

        Prepare();
    }


    public MessageClient(string apiKey, Uri proxyUrl, NetworkCredential credential = null)
    {
        ApiKey = apiKey;

        var proxy = new WebProxy(proxyUrl)
        {
            Credentials = credential
        };

        var httpClient = new HttpClient(
            new HttpClientHandler { Proxy = proxy, UseProxy = true }
        );

        TelegramClient = new TelegramBotClient(apiKey, httpClient);

        Prepare();
    }

    /// <summary>
    ///     Initializes the client with a proxy
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="proxyHost">i.e. 127.0.0.1</param>
    /// <param name="proxyPort">i.e. 10000</param>
    public MessageClient(string apiKey, string proxyHost, int proxyPort)
    {
        ApiKey = apiKey;

        var proxy = new WebProxy(proxyHost, proxyPort);

        var httpClient = new HttpClient(
            new HttpClientHandler { Proxy = proxy, UseProxy = true }
        );

        TelegramClient = new TelegramBotClient(apiKey, httpClient);

        Prepare();
    }


    public MessageClient(string apiKey, TelegramBotClient client)
    {
        ApiKey = apiKey;
        TelegramClient = client;

        Prepare();
    }




    public void Prepare()
    {
        TelegramClient.Timeout = new TimeSpan(0, 0, 30);
    }


    public void StartReceiving()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions();

        receiverOptions.ThrowPendingUpdates = ThrowPendingUpdates;

        if (UseThreadPool)
        {
            ThreadPool.SetMaxThreads(ThreadPool_WorkerThreads, ThreadPool_IOThreads);

            TelegramClient.StartReceiving(HandleUpdateAsyncThreadPool, HandleErrorAsyncThreadPool, receiverOptions,
                                     _cancellationTokenSource.Token);
        }
        else
        {
            TelegramClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions,
                                     _cancellationTokenSource.Token);
        }

    }

    public void StopReceiving()
    {
        _cancellationTokenSource.Cancel();
    }

    #region "Single Thread"

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        await OnMessageLoop(new UpdateResult(update, null));
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
                                 CancellationToken cancellationToken)
    {
        await OnReceiveError(new ErrorResult(exception));
    }

    #endregion

    #region "Thread Pool"

    public Task HandleUpdateAsyncThreadPool(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        ThreadPool.QueueUserWorkItem(async a =>
        {
            await OnMessageLoop(new UpdateResult(update, null));
        });

        return Task.CompletedTask;
    }

    public Task HandleErrorAsyncThreadPool(ITelegramBotClient botClient, Exception exception,
                             CancellationToken cancellationToken)
    {
        ThreadPool.QueueUserWorkItem(async a =>
        {
            await OnReceiveError(new ErrorResult(exception));
        });

        return Task.CompletedTask;
    }

    #endregion

    /// <summary>
    ///     This will return the current list of bot commands.
    /// </summary>
    /// <returns></returns>
    public async Task<BotCommand[]> GetBotCommands(BotCommandScope scope = null, string languageCode = null)
    {
        return await TelegramClient.GetMyCommandsAsync(scope, languageCode);
    }


    /// <summary>
    ///     This will set your bot commands to the given list.
    /// </summary>
    /// <param name="botcommands"></param>
    /// <param name="languageCode"></param>
    /// <returns></returns>
    public async Task SetBotCommands(List<BotCommand> botcommands, BotCommandScope scope = null,
                                     string languageCode = null)
    {
        await TelegramClient.SetMyCommandsAsync(botcommands, scope, languageCode);
    }

    /// <summary>
    ///     This will delete the current list of bot commands.
    /// </summary>
    /// <returns></returns>
    public async Task DeleteBotCommands(BotCommandScope scope = null, string languageCode = null)
    {
        await TelegramClient.DeleteMyCommandsAsync(scope, languageCode);
    }


    #region "Events"

    public event Async.AsyncEventHandler<UpdateResult> MessageLoop
    {
        add => Events.AddHandler(EvOnMessageLoop, value);
        remove => Events.RemoveHandler(EvOnMessageLoop, value);
    }

    public async Task OnMessageLoop(UpdateResult update)
    {
        var eventHandlers = (Events[EvOnMessageLoop] as Async.AsyncEventHandler<UpdateResult>)?.Invoke(this, update);

        if (eventHandlers != null)
        {
            await eventHandlers;
        }
    }


    public event Async.AsyncEventHandler<ErrorResult> ReceiveError
    {
        add => Events.AddHandler(EvOnReceiveError, value);
        remove => Events.RemoveHandler(EvOnReceiveError, value);
    }

    public async Task OnReceiveError(ErrorResult update)
    {
        var eventHandlers = (Events[EvOnReceiveError] as Async.AsyncEventHandler<ErrorResult>)?.Invoke(this, update);

        if (eventHandlers != null)
        {
            await eventHandlers;
            return;
        }

        //Fallback when no event handler is used.
        if (update.Exception is ApiRequestException exApi)
        {
            Console.WriteLine($"Telegram API Error:\n[{exApi.ErrorCode}]\n{exApi.Message}");
        }
        else
        {
            Console.WriteLine(update.Exception.ToString());
        }

    }

    #endregion
}
