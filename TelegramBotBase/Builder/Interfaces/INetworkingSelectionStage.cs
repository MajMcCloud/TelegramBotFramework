using System.Net.Http;
using Telegram.Bot;

namespace TelegramBotBase.Builder.Interfaces;

public interface INetworkingSelectionStage
{
    /// <summary>
    ///     Chooses a proxy as network configuration.
    /// </summary>
    /// <param name="proxyAddress"></param>
    /// <param name="throwPendingUpdates">Indicates if all pending Telegram.Bot.Types.Updates should be thrown out before start polling.</param>
    /// <returns></returns>
    IBotCommandsStage WithProxy(string proxyAddress, bool throwPendingUpdates = false, int timeoutInSeconds = 60);

    /// <summary>
    ///     Do not choose a proxy as network configuration.
    /// </summary>
    /// <param name="throwPendingUpdates">Indicates if all pending Telegram.Bot.Types.Updates should be thrown out before start polling.</param>
    /// <returns></returns>
    IBotCommandsStage NoProxy(bool throwPendingUpdates = false, int timeoutInSeconds = 60);


    /// <summary>
    ///     Chooses a custom instance of TelegramBotClient.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="throwPendingUpdates">Indicates if all pending Telegram.Bot.Types.Updates should be thrown out before start polling.</param>
    /// <returns></returns>
    IBotCommandsStage WithBotClient(TelegramBotClient client, bool throwPendingUpdates = false, int timeoutInSeconds = 60);


    /// <summary>
    ///     Sets the custom proxy host and port.
    /// </summary>
    /// <param name="proxyHost"></param>
    /// <param name="Port"></param>
    /// <param name="throwPendingUpdates">Indicates if all pending Telegram.Bot.Types.Updates should be thrown out before start polling.</param>
    /// <returns></returns>
    IBotCommandsStage WithHostAndPort(string proxyHost, int Port, bool throwPendingUpdates = false, int timeoutInSeconds = 60);

    /// <summary>
    ///     Uses a custom http client.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="throwPendingUpdates">Indicates if all pending Telegram.Bot.Types.Updates should be thrown out before start polling.</param>
    /// <returns></returns>
    IBotCommandsStage WithHttpClient(HttpClient client, bool throwPendingUpdates = false, int timeoutInSeconds = 60);
}