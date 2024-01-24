using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
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
    private EventHandlerList Events { get; } = new();

    private static readonly object EvOnMessageLoop = new();
    private static readonly object EvOnReceiveError = new();

    protected CancellationTokenSource _cancellationTokenSource;

    public string ApiKey { get; }

    public ITelegramBotClient TelegramClient { get; set; }



    /// <summary>
    ///    Indicates if all pending Telegram.Bot.Types.Updates should be thrown out before
    //     start polling. If set to true Telegram.Bot.Polling.ReceiverOptions.AllowedUpdates
    //     should be set to not null, otherwise Telegram.Bot.Polling.ReceiverOptions.AllowedUpdates
    //     will effectively be set to receive all Telegram.Bot.Types.Updates.
    /// </summary>
    public bool ThrowPendingUpdates { get; set; }


    public MessageClient(string apiKey)
    {
        ApiKey = apiKey;
        TelegramClient = new TelegramBotClient(apiKey);
    }

    public MessageClient(string apiKey, HttpClient proxy)
    {
        ApiKey = apiKey;
        TelegramClient = new TelegramBotClient(apiKey, proxy);
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
    }


    public MessageClient(string apiKey, TelegramBotClient client)
    {
        ApiKey = apiKey;
        TelegramClient = client;
    }


    public virtual void StartReceiving()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions();

        receiverOptions.ThrowPendingUpdates = ThrowPendingUpdates;

        TelegramClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, _cancellationTokenSource.Token);
    }


    public virtual void StopReceiving()
    {
        _cancellationTokenSource.Cancel();
    }


    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        await OnMessageLoop(new UpdateResult(update, null));
    }


    private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
                                 CancellationToken cancellationToken)
    {
        await OnReceiveError(new ErrorResult(exception));
    }


    #region "BotCommands"
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

    #endregion


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
