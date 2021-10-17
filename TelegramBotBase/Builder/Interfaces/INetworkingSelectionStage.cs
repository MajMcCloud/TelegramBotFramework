using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Telegram.Bot;

namespace TelegramBotBase.Builder.Interfaces
{
    public interface INetworkingSelectionStage
    {
        /// <summary>
        /// Chooses a proxy as network configuration.
        /// </summary>
        /// <param name="proxyAddress"></param>
        /// <returns></returns>
        IBuildingStage WithProxy(String proxyAddress);

        /// <summary>
        /// Do not choose a proxy as network configuration.
        /// </summary>
        /// <returns></returns>
        IBuildingStage NoProxy();


        /// <summary>
        /// Chooses a custom instance of TelegramBotClient.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        IBuildingStage WithBotClient(TelegramBotClient client);

        /// <summary>
        /// Sets the custom proxy host and port.
        /// </summary>
        /// <param name="proxyHost"></param>
        /// <param name="Port"></param>
        /// <returns></returns>
        IBuildingStage WithHostAndPort(String proxyHost, int Port);

        /// <summary>
        /// Uses a custom http client.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        IBuildingStage WithHttpClient(HttpClient client);


    }
}
