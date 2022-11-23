using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotBase.Base;
using TelegramBotBase.Builder.Interfaces;
using TelegramBotBase.Commands;
using TelegramBotBase.Factories;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Localizations;
using TelegramBotBase.States;

namespace TelegramBotBase.Builder
{
    public class BotBaseBuilder : IAPIKeySelectionStage, IMessageLoopSelectionStage, IStartFormSelectionStage, IBuildingStage, INetworkingSelectionStage, IBotCommandsStage, ISessionSerializationStage, ILanguageSelectionStage
    {

        String _apiKey = null;

        IStartFormFactory _factory = null;

        MessageClient _client = null;

        /// <summary>
        /// Contains different Botcommands for different areas.
        /// </summary>
        Dictionary<BotCommandScope, List<BotCommand>> _BotCommandScopes { get; set; } = new Dictionary<BotCommandScope, List<BotCommand>>();

        //List<BotCommand> _botcommands = new List<BotCommand>();

        IStateMachine _statemachine = null;

        IMessageLoopFactory _messageloopfactory = null;

        private BotBaseBuilder()
        {

        }

        public static IAPIKeySelectionStage Create()
        {
            return new BotBaseBuilder();
        }

        #region "Step 1 (Basic Stuff)"

        public IMessageLoopSelectionStage WithAPIKey(string apiKey)
        {
            this._apiKey = apiKey;
            return this;
        }


        public IBuildingStage QuickStart(string apiKey, Type StartForm)
        {
            this._apiKey = apiKey;
            this._factory = new Factories.DefaultStartFormFactory(StartForm);

            DefaultMessageLoop();

            NoProxy();

            OnlyStart();

            NoSerialization();

            DefaultLanguage();

            return this;
        }


        public IBuildingStage QuickStart<T>(string apiKey)
            where T : FormBase
        {
            this._apiKey = apiKey;
            this._factory = new Factories.DefaultStartFormFactory(typeof(T));

            DefaultMessageLoop();

            NoProxy();

            OnlyStart();

            NoSerialization();

            DefaultLanguage();

            return this;
        }

        public IBuildingStage QuickStart(string apiKey, IStartFormFactory StartFormFactory)
        {
            this._apiKey = apiKey;
            this._factory = StartFormFactory;

            DefaultMessageLoop();

            NoProxy();

            OnlyStart();

            NoSerialization();

            DefaultLanguage();

            return this;
        }

        #endregion


        #region "Step 2 (Message Loop)"

        public IStartFormSelectionStage DefaultMessageLoop()
        {
            _messageloopfactory = new MessageLoops.FormBaseMessageLoop();

            return this;
        }


        public IStartFormSelectionStage MinimalMessageLoop()
        {
            _messageloopfactory = new MessageLoops.MinimalMessageLoop();

            return this;
        }


        public IStartFormSelectionStage CustomMessageLoop(IMessageLoopFactory messageLoopClass)
        {
            _messageloopfactory = messageLoopClass;

            return this;
        }

        public IStartFormSelectionStage CustomMessageLoop<T>()
            where T : class, new()
        {
            _messageloopfactory = typeof(T).GetConstructor(new Type[] { })?.Invoke(new object[] { }) as IMessageLoopFactory;

            return this;
        }

        #endregion


        #region "Step 3 (Start Form/Factory)"

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

        public INetworkingSelectionStage WithServiceProvider(Type startFormClass, IServiceProvider serviceProvider)
        {
            this._factory = new ServiceProviderStartFormFactory(startFormClass, serviceProvider);
            return this;
        }

        public INetworkingSelectionStage WithServiceProvider<T>(IServiceProvider serviceProvider)
            where T : FormBase
        {
            this._factory = new ServiceProviderStartFormFactory<T>(serviceProvider);
            return this;
        }

        public INetworkingSelectionStage WithStartFormFactory(IStartFormFactory factory)
        {
            this._factory = factory;
            return this;
        }

        #endregion


        #region "Step 4 (Network Settings)"

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


        #endregion


        #region "Step 5 (Bot Commands)"

        public ISessionSerializationStage NoCommands()
        {
            return this;
        }

        public ISessionSerializationStage OnlyStart()
        {
            _BotCommandScopes.Start("Starts the bot");

            return this;

        }

        public ISessionSerializationStage DefaultCommands()
        {
            _BotCommandScopes.Start("Starts the bot");
            _BotCommandScopes.Help("Should show you some help");
            _BotCommandScopes.Settings("Should show you some settings");
            return this;
        }

        public ISessionSerializationStage CustomCommands(Action<Dictionary<BotCommandScope, List<BotCommand>>> action)
        {
            action?.Invoke(_BotCommandScopes);
            return this;
        }

        #endregion


        #region "Step 6 (Serialization)"

        public ILanguageSelectionStage NoSerialization()
        {
            return this;
        }

        public ILanguageSelectionStage UseSerialization(IStateMachine machine)
        {
            this._statemachine = machine;
            return this;
        }


        public ILanguageSelectionStage UseJSON(string path)
        {
            this._statemachine = new JSONStateMachine(path);
            return this;
        }

        public ILanguageSelectionStage UseSimpleJSON(string path)
        {
            this._statemachine = new SimpleJSONStateMachine(path);
            return this;
        }

        public ILanguageSelectionStage UseXML(string path)
        {
            this._statemachine = new XMLStateMachine(path);
            return this;
        }

        #endregion


        #region "Step 7 (Language)"

        public IBuildingStage DefaultLanguage()
        {
            return this;
        }

        public IBuildingStage UseEnglish()
        {
            Localizations.Default.Language = new Localizations.English();
            return this;
        }

        public IBuildingStage UseGerman()
        {
            Localizations.Default.Language = new Localizations.German();
            return this;
        }

        public IBuildingStage Custom(Localization language)
        {
            Localizations.Default.Language = language;
            return this;
        }

        #endregion


        public BotBase Build()
        {
            var bb = new BotBase();

            bb.APIKey = _apiKey;
            bb.StartFormFactory = _factory;

            bb.Client = _client;

            bb.BotCommandScopes = _BotCommandScopes;

            bb.StateMachine = _statemachine;

            bb.MessageLoopFactory = _messageloopfactory;

            bb.MessageLoopFactory.UnhandledCall += bb.MessageLoopFactory_UnhandledCall;

            return bb;
        }

    }
}
