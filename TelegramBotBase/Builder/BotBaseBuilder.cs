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
using TelegramBotBase.States;

namespace TelegramBotBase.Builder
{
    public class BotBaseBuilder : IAPIKeySelectionStage, IMessageLoopSelectionStage, IStartFormSelectionStage, IBuildingStage, INetworkingSelectionStage, IBotCommandsStage, ISessionSerializationStage
    {

        String _apiKey = null;

        IStartFormFactory _factory = null;

        MessageClient _client = null;

        List<BotCommand> _botcommands = new List<BotCommand>();

        IStateMachine _statemachine = null;

        IMessageLoopFactory _messageloopfactory = null;

        public static IAPIKeySelectionStage Create()
        {
            return new BotBaseBuilder();
        }

        #region "Step 1"

        public IMessageLoopSelectionStage WithAPIKey(string apiKey)
        {
            this._apiKey = apiKey;
            return this;
        }

        #endregion


        #region "Step 2"

        public IStartFormSelectionStage DefaultMessageLoop()
        {
            _messageloopfactory = new Factories.MessageLoops.FormBaseMessageLoop();

            return this;
        }

        public IStartFormSelectionStage CustomMessageLoop(Type messageLoopClass)
        {
            if (messageLoopClass.IsSubclassOf(typeof(IMessageLoopFactory)))
                throw new ArgumentException($"Not a subclass of {nameof(IMessageLoopFactory)}");

            _messageloopfactory = messageLoopClass.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as IMessageLoopFactory;

            return this;
        }

        public IStartFormSelectionStage CustomMessageLoop<T>()
            where T : class, new()
        {
            _messageloopfactory = typeof(T).GetConstructor(new Type[] { })?.Invoke(new object[] { }) as IMessageLoopFactory;

            return this;
        }

        #endregion

        #region "Step 3"

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

        #endregion


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


        public IBuildingStage UseJSON(string path)
        {
            this._statemachine = new JSONStateMachine(path);
            return this;
        }

        public IBuildingStage UseSimpleJSON(string path)
        {
            this._statemachine = new SimpleJSONStateMachine(path);
            return this;
        }

        public IBuildingStage UseXML(string path)
        {
            this._statemachine = new XMLStateMachine(path);
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

            bb.MessageLoopFactory = _messageloopfactory;

            bb.MessageLoopFactory.UnhandledCall += bb.MessageLoopFactory_UnhandledCall;

            return bb;
        }

    }
}
