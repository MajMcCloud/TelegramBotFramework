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
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Base;

/// <summary>
///     Base class for message handling
/// </summary>
public class ThreadPoolMessageClient : MessageClient
{

    /// <summary>
    ///    Indicates if all pending Telegram.Bot.Types.Updates should be thrown out before
    //     start polling. If set to true Telegram.Bot.Polling.ReceiverOptions.AllowedUpdates
    //     should be set to not null, otherwise Telegram.Bot.Polling.ReceiverOptions.AllowedUpdates
    //     will effectively be set to receive all Telegram.Bot.Types.Updates.
    /// </summary>

    public int ThreadPool_WorkerThreads { get; set; } = 1;

    public int ThreadPool_IOThreads { get; set; } = 1;


    public ThreadPoolMessageClient(string apiKey) : base(apiKey)
    {

    }

    public ThreadPoolMessageClient(string apiKey, HttpClient proxy) : base(apiKey, proxy)
    {

    }


    public ThreadPoolMessageClient(string apiKey, Uri proxyUrl, NetworkCredential credential = null) : base(apiKey, proxyUrl, credential)
    {

    }

    /// <summary>
    ///     Initializes the client with a proxy
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="proxyHost">i.e. 127.0.0.1</param>
    /// <param name="proxyPort">i.e. 10000</param>
    public ThreadPoolMessageClient(string apiKey, string proxyHost, int proxyPort) : base(apiKey, proxyHost, proxyPort)
    {

    }


    public ThreadPoolMessageClient(string apiKey, TelegramBotClient client) : base(apiKey, client)
    {

    }



    public override void StartReceiving()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions();

        receiverOptions.ThrowPendingUpdates = ThrowPendingUpdates;

        ThreadPool.SetMaxThreads(ThreadPool_WorkerThreads, ThreadPool_IOThreads);
        
        TelegramClient.StartReceiving(HandleUpdateAsyncThreadPool, HandleErrorAsyncThreadPool, receiverOptions, _cancellationTokenSource.Token);
    }

    public override void StopReceiving()
    {
        _cancellationTokenSource.Cancel();
    }


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

}
