using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotBase.Base;
using TelegramBotBase.Builder.Interfaces;
using TelegramBotBase.Commands;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Builder
{
    public class BotBaseBuilder : IAPIKeySelectionStage, IStartFormSelectionStage, IBuildingStage, INetworkingSelectionStage, IBotCommandsStage, ISessionSerializationStage
    {

        String _apiKey = null;

        IStartFormFactory _factory = null;

        MessageClient _client = null;

        List<BotCommand> _botcommands = new List<BotCommand>();

        IStateMachine _statemachine = null;

        public static IAPIKeySelectionStage Create()
        {
            return new BotBaseBuilder();
        }

        public IStartFormSelectionStage WithAPIKey(string apiKey)
        {
            this._apiKey = apiKey;
            return this;
        }

        public INetworkingSelectionStage WithStartForm(Type startFormClass)
        {
            this._factory = new Factories.DefaultStartFormFactory(startFormClass);
            return this;
        }

        public INetworkingSelectionStage WithStartForm<T>()
            where T : FormBase, new()
        {
            this._factory = new Factories.DefaultStartFormFactory(typeof(T));
            return this;
        }

        public INetworkingSelectionStage WithStartFormFactory(IStartFormFactory factory)
        {
            this._factory = factory;
            return this;
        }


        public IBotCommandsStage WithProxy(string proxyAddress)
        {
            var url = new Uri(proxyAddress);
            _client = new MessageClient(_apiKey, url);
            _client.TelegramClient.Timeout = new TimeSpan(0, 1, 0);
            return this;
        }

        public IBotCommandsStage NoProxy()
        {
            _client = new MessageClient(_apiKey);
            _client.TelegramClient.Timeout = new TimeSpan(0, 1, 0);
            return this;
        }


        public IBotCommandsStage WithBotClient(TelegramBotClient tgclient)
        {
            _client = new MessageClient(_apiKey, tgclient);
            _client.TelegramClient.Timeout = new TimeSpan(0, 1, 0);
            return this;
        }

        public IBotCommandsStage WithHostAndPort(string proxyHost, int proxyPort)
        {
            _client = new MessageClient(_apiKey, proxyHost, proxyPort);
            _client.TelegramClient.Timeout = new TimeSpan(0, 1, 0);
            return this;
        }

        public IBotCommandsStage WithHttpClient(HttpClient tgclient)
        {
            _client = new MessageClient(_apiKey, tgclient);
            _client.TelegramClient.Timeout = new TimeSpan(0, 1, 0);
            return this;
        }

        public ISessionSerializationStage NoCommands()
        {
            return this;
        }

        public ISessionSerializationStage DefaultCommands()
        {
            _botcommands.AddStartCommand("Starts the bot");
            _botcommands.AddHelpCommand("Should show you some help");
            _botcommands.AddSettingsCommand("Should show you some settings");
            return this;
        }

        public ISessionSerializationStage CustomCommands(Action<List<BotCommand>> action)
        {
            action?.Invoke(_botcommands);
            return this;
        }


        public IBuildingStage NoSerialization()
        {
            return this;
        }

        public IBuildingStage UseSerialization(IStateMachine machine)
        {
            this._statemachine = machine;
            return this;
        }

        public BotBase Build()
        {
            var bb = new BotBase();

            bb.APIKey = _apiKey;
            bb.StartFormFactory = _factory;

            bb.Client = _client;

            bb.Sessions.Client = bb.Client;

            bb.BotCommands = _botcommands;

            bb.StateMachine = _statemachine;

            return bb;
        }

    }
}
