using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Telegram.Bot;
using TelegramBotBase.Base;
using TelegramBotBase.Builder.Interfaces;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Builder
{
    public class BotBaseBuilder : IAPIKeySelectionStage, IStartFormSelectionStage, IBuildingStage, INetworkingSelectionStage
    {

        String apiKey = null;

        IStartFormFactory factory = null;

        MessageClient client = null;

        public static IAPIKeySelectionStage Create()
        {
            return new BotBaseBuilder();
        }

        public IStartFormSelectionStage WithAPIKey(string apiKey)
        {
            this.apiKey = apiKey;
            return this;
        }

        public INetworkingSelectionStage WithStartForm(Type startFormClass)
        {
            this.factory = new Factories.DefaultStartFormFactory(startFormClass);
            return this;
        }

        public INetworkingSelectionStage WithStartForm<T>()
            where T : FormBase, new()
        {
            this.factory = new Factories.DefaultStartFormFactory(typeof(T));
            return this;
        }

        public INetworkingSelectionStage WithStartFormFactory(IStartFormFactory factory)
        {
            this.factory = factory;
            return this;
        }


        public IBuildingStage WithProxy(string proxyAddress)
        {
            var url = new Uri(proxyAddress);
            client = new MessageClient(apiKey, url);
            client.TelegramClient.Timeout = new TimeSpan(0, 1, 0);
            return this;
        }

        public IBuildingStage NoProxy()
        {
            client = new MessageClient(apiKey);
            client.TelegramClient.Timeout = new TimeSpan(0, 1, 0);
            return this;
        }


        public IBuildingStage WithBotClient(TelegramBotClient tgclient)
        {
            client = new MessageClient(apiKey, tgclient);
            client.TelegramClient.Timeout = new TimeSpan(0, 1, 0);
            return this;
        }

        public IBuildingStage WithHostAndPort(string proxyHost, int proxyPort)
        {
            client = new MessageClient(apiKey, proxyHost, proxyPort);
            client.TelegramClient.Timeout = new TimeSpan(0, 1, 0);
            return this;
        }

        public IBuildingStage WithHttpClient(HttpClient tgclient)
        {
            client = new MessageClient(apiKey, tgclient);
            client.TelegramClient.Timeout = new TimeSpan(0, 1, 0);
            return this;
        }

        public BotBase Build()
        {
            var bb = new BotBase();

            bb.APIKey = apiKey;
            bb.StartFormFactory = factory;

            bb.Client = client;

            bb.Sessions.Client = bb.Client;

            return bb;
        }

    }
}
