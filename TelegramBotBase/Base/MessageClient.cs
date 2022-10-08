using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace TelegramBotBase.Base;

/// <summary>
///     Base class for message handling
/// </summary>
public class MessageClient
{
    private static readonly object EvOnMessageLoop = new();

    private static object __evOnMessage = new();

    private static object __evOnMessageEdit = new();

    private static object __evCallbackQuery = new();

    private CancellationTokenSource _cancellationTokenSource;


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


    public string ApiKey { get; set; }

    public ITelegramBotClient TelegramClient { get; set; }

    private EventHandlerList Events { get; } = new();


    public void Prepare()
    {
        TelegramClient.Timeout = new TimeSpan(0, 0, 30);
    }


    public void StartReceiving()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions();

        TelegramClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions,
                                      _cancellationTokenSource.Token);
    }

    public void StopReceiving()
    {
        _cancellationTokenSource.Cancel();
    }


    public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        OnMessageLoop(new UpdateResult(update, null));

        return Task.CompletedTask;
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
                                 CancellationToken cancellationToken)
    {
        if (exception is ApiRequestException exApi)
        {
            Console.WriteLine($"Telegram API Error:\n[{exApi.ErrorCode}]\n{exApi.Message}");
        }
        else
        {
            Console.WriteLine(exception.ToString());
        }

        return Task.CompletedTask;
    }


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

    public void OnMessageLoop(UpdateResult update)
    {
        (Events[EvOnMessageLoop] as Async.AsyncEventHandler<UpdateResult>)?.Invoke(this, update);
    }

    #endregion
}